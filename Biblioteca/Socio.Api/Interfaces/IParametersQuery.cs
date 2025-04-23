using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socio.Api.Interfaces
{
    public interface IParametersQuery
    {
        public int? Top { get; set; }
        public string Filter { get; set; }
        public List<string> OrderBy { get; set; }

        public int? Page { get; set; }
        public int? Rows { get; set; }
    }
}
