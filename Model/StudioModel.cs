using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class StudioModel
    {
        [Key]
        public string? IdPaciente { get; set; }
        public string? IdProfesional { get; set; }
        public string? IdEntidad { get; set; }
        public string? TipoDoc { get; set; }
        public string? NroDoc { get; set; }
        public string? Apellido1 { get; set; }

        public string? Nombre1 { get; set; }
        public string? DV { get; set; }
        public string? NroAfil { get; set; }

        public string? Servicio { get; set; }
        public string? DíaReali { get; set; }
        public string? FechReal { get; set; }


        public string? NrodeOrden { get; set; }
        public string? Estado { get; set; }
        public string? NOMBRE_PDF { get; set; }
        public string? Link_PDF { get; set; }
        public string? Link_Imagenes { get; set; }
        public string? Link_Imagenes1 { get; set; }
        public string? Link_Imagenes2 { get; set; }
        public string? EMAIL { get; set; }
        public string? FechaNac { get; set; }

    }
}
