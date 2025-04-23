using Microsoft.AspNetCore.Mvc.Infrastructure;
using Socio.Api.Model;
using Socio.Api.Result;
using System.Net;

namespace Socio.Api.Interfaces
{
    public interface IApiResult
    {
        bool HasError { get; set; }
        string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; }

        public void Set(string Message, bool Error = false);
        public void Set(IApiResult From);
        public void AppendMessage(string line);
    }

    public interface IApiResult <T> : IApiResult 
    {
        public T Data { get; set; }
    }

}
