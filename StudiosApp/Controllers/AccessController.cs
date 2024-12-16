using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.DTOs;
using Model;
using Repository;
using Model.Custom;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using static Model.Enums;
using Azure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net.Mail;
using System.Net;

namespace StudiosApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {

        private readonly IRegistroService _registroService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly Usefulness _usefulness;

        public AccessController(IRegistroService registroService, IAuditoriaService auditoriaService, Usefulness usefulness)
        {
            _registroService = registroService;
            _auditoriaService = auditoriaService;
            _usefulness = usefulness;
        }
        #region Metodos privados

        private Auditoria CrearAuditoria(string detalle, bool resultado, Enums.Protocolo protocolo, RegistroDto registro)
        {
            string ipAddress = GetClientIpAddress();
            string userAgent = Request?.Headers?["User-Agent"].ToString() ?? "Desconocido";

            return _usefulness.CrearAuditoria(ipAddress, userAgent, detalle, resultado, protocolo, registro);
        }

        private string GetClientIpAddress()
        {
            var remoteIpAddress = HttpContext?.Connection?.RemoteIpAddress;

            if (remoteIpAddress == null)
            {
                return "IP no disponible";
            }

            // Verificar si es una IP local (loopback)
            if (IPAddress.IsLoopback(remoteIpAddress))
            {
                // Obtener la IP de la máquina local
                var localIp = Dns.GetHostEntry(Dns.GetHostName())
                                  .AddressList
                                  .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                return localIp?.ToString() ?? "IP local no disponible";
            }

            // Si no es loopback, devolver la IP remota
            return remoteIpAddress.ToString();
        }
        private void EnviarMail(RegistroDto user, bool crear)
        {
            try
            {
                string final = $@"
    <p>Su contraseña provisoria es: <b style='background-color: yellow;'>{user.Clave}</b></p>
    <p>Si tiene alguna pregunta o necesita asistencia, puede contactarnos a nuestro 
    <a href='https://wa.me/541128080002' target='_blank'>WhatsApp 1128080002</a></p>
    <p>¡Gracias por confiar en nosotros!</p>
    <p>Centro Moreau<br />
    <a href='http://www.centromoreau.com.ar' target='_blank'>www.centromoreau.com.ar</a></p>";

                string asunto = crear ?
                    "Portal Web Centro Moreau - Alta de Usuario" :
                    "Portal Web Centro Moreau - Restablecimiento de Usuario";

                string cuerpo = crear ?
                    $@"
    <p>Bienvenido/a al Portal Web Centro Moreau.</p>
    <p>Nos complace ofrecerle un acceso fácil y seguro para la visualización de estudios, 
    con la calidad y confianza que nos caracteriza. Este portal está diseñado para facilitar su 
    acceso a la información que necesita, priorizando siempre la seguridad y privacidad de los datos.</p>
    {final}" :
                    $@"
    <p>Estimado/a: {user.NombreApellido}</p>
    <p>Su solicitud de restablecimiento de contraseña para el Portal Web Centro Moreau ha sido procesada.</p>
    {final}";


               
                SmtpClient smtpClient = new SmtpClient("smtp-relay.gmail.com") // Cambia por el nombre o IP del servidor
                {
                    Port = 25, // Puerto común para SMTP Relay (puede ser 587 o 2525 en algunos casos)
                    EnableSsl = false, // Usualmente los relays internos no requieren SSL/TLS
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true // Usa las credenciales predeterminadas del sistema
                };

               
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("info@centromoreau.com.ar", "Centro Moreau"),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = true 
                };

               
                mailMessage.To.Add(user.Correo);
                //mailMessage.To.Add(user.Correo); 

                // Enviar el correo
                smtpClient.Send(mailMessage);
                _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(Auditoria.Envio_Mail, true, Enums.Protocolo.Envio_Mail, user));

            }
            catch (Exception ex)
            {
                _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(Auditoria.Envio_Mail, false, Enums.Protocolo.Envio_Mail, user));
            }
        }
        private Boolean ExistMovements(string id, int type, string? email = null, string? fechanacimiento = null)
        {

            var result = _registroService.GetUserTypeAsync(id.Trim(), type, email, fechanacimiento)?.GetAwaiter().GetResult();
            return result?.Count() > 0;

        }

        private Boolean ExistUser(string id, int tipo, string? email)
        {
            var resultado = _registroService.GetUserUpdateAsync(id, tipo, email)?.GetAwaiter().GetResult();

            return resultado?.Count() == 1;
        }

        private RegistroModel Login(RegistroDto usuario)
        {
            return _registroService.GetUserAsync(usuario)?.Result?.FirstOrDefault() ?? null;

        }

        private object ErroresModelValue(ModelStateDictionary modelState)
        {
            var errors = modelState.Values
        .SelectMany(v => v.Errors)
        .Select(e => e.ErrorMessage)
        .ToList();

            return new { Errors = errors };

        }

        public static string GenerarPasswordProvisoria()
        {
            string guid = Guid.NewGuid().ToString("N");
            return guid.Substring(0, 6);
        }
        #endregion
        #region POST
        [HttpPost("PostUserToken")]
        public async Task<ActionResult<RegistroModel>> PostUserAsync(RegistroDto usuario)
        {
            try
            {
                if (!this.ExistUser(usuario.IdRegistro, usuario.Tipo, null))
                    throw new Exception("Usuario Inexistente.");

                RegistroModel registro = Login(usuario);
                bool hayinicio = registro != null;
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(Auditoria.Login, hayinicio, Enums.Protocolo.Login, usuario));
                if (hayinicio)
                    return StatusCode(StatusCodes.Status200OK, new ResponseDTO { isSuccess = true, token = _usefulness.generarJWT(usuario), Usuario = new RegistroDto() { IdRegistro = registro.IdRegistro, Tipo = registro.Tipo, Correo = registro.Correo, Clave = usuario.Clave, DebeCambiarPassword = registro.DebeCambiarPassword } });

                throw new Exception("Inicio de sesión incorrecto, verifique su contraseña");
            }
            catch (Exception ex)
            {
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(ex.Message, false, Enums.Protocolo.Intento_Inicio_Sesion, usuario));
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("AddUser")]
        public async Task<ActionResult> AddUserAsync(RegistroDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErroresModelValue(ModelState));

            try
            {
                // Verificar si el usuario ya existe
                if (this.ExistUser(user.IdRegistro, user.Tipo, null))
                    throw new Exception("Usuario Existente.");

                // Verificar movimientos en la base de datos
                if (!this.ExistMovements(user.IdRegistro, user.Tipo))
                    throw new Exception("No se encontraron registros de estudios, por favor comuníquese con el Centro.");

                if (user.Tipo == Tipos.Paciente.GetHashCode())
                {

                    if (DateTime.TryParse(user.FechaNacimiento, out DateTime fechaNacimiento) && fechaNacimiento.Equals(DateTime.MinValue))
                        throw new Exception("Error en obtener la fecha de nacimiento del paciente.");

                    if (!this.ExistMovements(user.IdRegistro, user.Tipo, null, fechaNacimiento.ToString("dd/MM/yyyy")))
                        throw new Exception("La fecha de nacimiento ingresada no corresponde al dado de alta como paciente. Por favor comuníquese con el Centro para continuar.");
                }
                // Verificar el email en la base de datos
                if (!this.ExistMovements(user.IdRegistro, user.Tipo, user.Correo, null))
                    throw new Exception("El email ingresado no corresponde al dado de alta. Por favor comuníquese con el Centro para confirmar su email.");

                user.Clave = GenerarPasswordProvisoria();

                // Crear el modelo y asignar propiedades, incluyendo las fechas
                var model = new RegistroModel
                {
                    IdRegistro = user.IdRegistro,
                    Correo = user.Correo,
                    Clave = user.Clave,
                    Tipo = user.Tipo,
                    DebeCambiarPassword = true,
                    FechaCreacion = DateTime.Now, // Asignar la fecha de creación
                    FechaModificacion = DateTime.Now // Asignar la fecha de modificación
                };

                var response = await _registroService.AddUserAsync(model);
                bool registrofinalizado = response == 1;
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(Auditoria.Alta_Usuario, registrofinalizado, Enums.Protocolo.Alta_Usuario, user));
                if (!registrofinalizado)
                    return BadRequest("No se pudo crear el usuario, intente nuevamente.");

                EnviarMail(user, true); // Envía la clave desencriptada             
                return Ok(1);
            }
            catch (Exception ex)
            {
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(ex.Message, false, Enums.Protocolo.Intento_Alta_Usuario, user));
                return BadRequest(ex.Message);
            }
        }


        #endregion
        #region GET

        [HttpGet("GetAlive")]
        public IActionResult GetAliveAsync()
        {
            return Ok("VIVO");
        }

        #endregion
        #region PUT
        [HttpPut("ChangeForgottenPass")]
        public async Task<ActionResult> ChangeForgottenPassAsync(RegistroDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErroresModelValue(ModelState));

            try
            {

                if (!this.ExistUser(user.IdRegistro, user.Tipo, null))
                    throw new Exception("Usuario Inexistente.");

                //si existen movimientos en base de datos del centro, el tipo determina el sp a ejecutar OK!!

                if (!this.ExistUser(user.IdRegistro, user.Tipo, user.Correo))
                    throw new Exception("El email ingresado no corresponde al dado de alta. Por favor comuníquese con el Centro para confirmar su email.");

                if (user.Tipo == Tipos.Paciente.GetHashCode())
                {

                    if (DateTime.TryParse(user.FechaNacimiento, out DateTime result) && result.Equals(DateTime.MinValue))
                        throw new Exception("Error en obtener la fecha de nacimiento del paciente");

                    if (!this.ExistMovements(user.IdRegistro, user.Tipo, null, result.ToString("dd/MM/yyyy")))
                        throw new Exception("La fecha de nacimiento ingresada no corresponde al dado de alta como paciente. Por favor comuníquese con el Centro para continuar.");

                }
                user.DebeCambiarPassword = true;
                user.Clave = GenerarPasswordProvisoria();
                var response = await _registroService.UpdateUserAsync(user);
                bool registrofinalizado = response == 1;
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(Auditoria.Recuperar_Contraseña, registrofinalizado, Enums.Protocolo.Recuperar_Contraseña, user));
                if (registrofinalizado)
                    EnviarMail(user, false); // Envía la clave desencriptada
                else
                    throw new Exception("No se pudo recuperar la contraeña, intente nuevamente.");



                return Ok(response);
            }
            catch (Exception ex)
            {
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(ex.Message, false, Enums.Protocolo.Intento_Recuperar_Contraseña, user));
                return BadRequest(ex.Message);
            }

        }



        [HttpPut("updateuser")]
        public async Task<ActionResult> UpdateUserAsync(RegistroDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErroresModelValue(ModelState));

            try
            {
                user.DebeCambiarPassword = false;
                var response = await _registroService.UpdateUserAsync(user);
                bool hubocambio = response == 1;
                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(Auditoria.Cambiar_Contraseña, hubocambio, Enums.Protocolo.Cambiar_Contraseña, user));
                if (!hubocambio)
                    return BadRequest("No se pudo modificar la contraseña, intente nuevamente.");

                return Ok(response);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region DELETE
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUserAsync(string? id, int? tipo)
        {
            if (id == null || tipo.HasValue == false)
                return BadRequest("Sin información brindada, no se puede continuar");
            try
            {
                var response = await _registroService.DeleteUserAsync(id, tipo.Value);
                if (response == 0)
                    return BadRequest("No eliminado, intente nuevamente más tarde");
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion


    }
}
