using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTOs
{
    public class ResponseDTO
    {
        public bool isSuccess { get; set; }
        public string token { get; set; } = "";
        public RegistroDto Usuario { get; set; } = new RegistroDto();
    }
}
