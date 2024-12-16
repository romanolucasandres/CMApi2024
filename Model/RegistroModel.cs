using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
	public class RegistroModel
	{
		[Key]
		public string IdRegistro { get; set; } = "";
		public string Correo { get; set; } = "";
		public string Clave { get; set; } = "";
		public bool DebeCambiarPassword { get; set; } = false;
        public int Tipo { get; set; }

		public DateTime FechaCreacion { get; set; }
		public DateTime FechaModificacion { get; set; }
    }
}
