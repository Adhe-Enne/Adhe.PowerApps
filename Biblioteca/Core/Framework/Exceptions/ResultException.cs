using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework.Exceptions
{
    public class ResultException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public ResultException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
