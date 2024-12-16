using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Custom;
using Model.DTOs;
using Repository;
using System.Net;

namespace StudiosApp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class StudioController : ControllerBase
    {
        private readonly IStudioService _studioService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly Usefulness _usefulness;

        public StudioController(IStudioService studioService, IAuditoriaService auditoriaService,Usefulness usefulness)
        {
            _studioService = studioService;
            _auditoriaService = auditoriaService;
            _usefulness = usefulness;
        }

        #region Metodos Privados
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
        #endregion

        [HttpGet("GetStudiosList")]
        public async Task<ActionResult> GetStudioListAsync(string id)
        {
            try
            {
                List<StudioModel> lista = await _studioService.GetStudioListAsync(id);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetStudiosProfesionalList")]
        public async Task<ActionResult> GetStudiosProfesionalListAsync(string idProfesional, string idPaciente)
        {
            try
            {
                List<StudioModel> lista = new List<StudioModel>();
                if (!string.IsNullOrEmpty(idPaciente))
                {
                    lista = await _studioService.GetStudiosProfesionalListAsync(idProfesional, idPaciente);
                    bool resultadook = lista != null;
                    await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(idPaciente, resultadook, Enums.Protocolo.Buscar_Paciente, new RegistroDto() { IdRegistro = idProfesional, Tipo = Enums.Tipos.Profesional.GetHashCode() }));
                    if (!resultadook)
                        lista = new List<StudioModel>();
                }
                return Ok(lista);

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }



        [HttpGet("GetStudiosEntidadList")]
        public async Task<ActionResult> GetStudiosEntidadListAsync(string idEntidad, string idPaciente)
        {
            try
            {
                List<StudioModel> lista = new List<StudioModel>();
                if (!string.IsNullOrEmpty(idPaciente))
                {
                    lista = await _studioService.GetStudiosEntidadListAsync(idEntidad, idPaciente);
                    bool resultadook = lista != null;
                    await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(idPaciente, resultadook, Enums.Protocolo.Buscar_Paciente, new RegistroDto() { IdRegistro = idEntidad, Tipo = Enums.Tipos.Entidad.GetHashCode() }));
                    if (!resultadook)
                        lista = new List<StudioModel>();
                }
                return Ok(lista);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
