using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libro.FunctionApp.Model
{
    public class ResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Content { get; set; }
    }
}
