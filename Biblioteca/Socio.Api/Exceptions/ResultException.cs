using System.Net;

namespace Crm.Access.Exceptions
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
