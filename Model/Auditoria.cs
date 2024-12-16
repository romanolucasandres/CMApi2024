using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Auditoria
    {
        public const string Alta_Usuario = "Usuario proceso de alta"; //ok
        public const string Cambiar_Contraseña = "Usuario cambio de contraseña"; //ok
        public const string Recuperar_Contraseña = "Usuario recupero de contraseña"; //ok
        public const string Login = "Usuario inicio de sesión";     //ok
        public const string Envio_Mail = "Intento de envio de mail de contraseña provisoria"; //ok

        [Key]
        public Guid Id { get; set; }
        public string? IdUsuario { get; set; }
        public string? IpUsuario { get; set; }
        public string? NombrePCUsuario { get; set; }       
        public int? TipoUsuario { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? Protocolo { get; set; }
        public string? Descripcion { get; set; }
        public bool? Resultado { get; set; }


        
    }
}
