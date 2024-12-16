using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTOs
{
	public class RegistroDto
	{
        [Key]
        [Required(ErrorMessage = "Id no ingresado")]
        
        public string IdRegistro { get; set; } = "";
      
        public string Correo { get; set; } = "";

        public string NombreApellido { get; set; } = "";
        public string Clave { get; set; } = "";
        [Required(ErrorMessage = "Tipo no ingresado")]
        public int Tipo { get; set; }
        public bool DebeCambiarPassword { get; set; } = false;
        public string? FechaNacimiento { get; set; }
        

    }
}
