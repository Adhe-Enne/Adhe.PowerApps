using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Common.Dto
{
    public class SocioPost
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public int? Dni { get; set; }
        public int? TipoTelefono { get; set; }
        public string? Telefono { get; set; }
    }
}
