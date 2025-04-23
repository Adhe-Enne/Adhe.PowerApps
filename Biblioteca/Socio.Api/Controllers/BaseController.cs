using AutoMapper;
using Crm.Access.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Socio.Api.Interfaces;
using Socio.Api.Result;
using System.Net;

namespace Socio.Api.Controllers
{
    public class BaseController : Controller
    {
        protected ILogger _log;
        protected IMapper _mapper;
        public BaseController(ILogger log, IMapper mapper)
        {
            _log = log;
            _mapper = mapper;
        }

        protected IApiResult HandleException(ResultException ex, string message = null)
        {
            message = $"{message ?? "Error"} : {ex.StatusCode} . Exception: {ex.Message}";
            _log.LogError(message, ex);

            return new ApiResult().SetError(message, ex.StatusCode);
        }

        protected IApiResult HandleException(Exception ex, HttpStatusCode statusCode, string message = null)
        {
            message = $"{message ?? "Error"} : {statusCode} . Exception: {ex.Message}";
            _log.LogError(message, ex);

            return new ApiResult().SetError(message, statusCode);
        }

        private string GetMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;

            return ex.Message;
        }
    }
}
