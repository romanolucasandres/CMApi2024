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
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        IAuditoriaService _auditoriaService;
        Usefulness _usefulness;
        public AuditoriaController(IAuditoriaService auditoriaService, Usefulness usefulness)
        {
            
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

        [HttpPost("PostAuditoria")]
        public async Task<ActionResult> PostAuditoriaAsync(RegistroDto user, string detalle, bool? pdf)
        {
            try
            {
                
                Enums.Protocolo protocolo = Enums.Protocolo.Generico;

                if (pdf.HasValue)
                    protocolo = Enums.Protocolo.Abrir_Link;

                await _auditoriaService.AddAuditoriaAsync(this.CrearAuditoria(detalle, true, protocolo, user));

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
