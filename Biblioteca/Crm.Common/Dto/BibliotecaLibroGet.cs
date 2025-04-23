using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Common.Dto
{
    public class BibliotecaLibroGet
    {
        public Guid Id { get; set; }
        public string Libro { get; set; }
        public string Biblioteca{ get; set; }
        public Guid LibroGuid { get; set; }
        public Guid BibliotecaGuid { get; set; }
        public int Unidades { get; set; }
    }
}
