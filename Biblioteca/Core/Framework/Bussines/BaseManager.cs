using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Framework.Exceptions;
using Microsoft.Extensions.Logging;

namespace Core.Framework.Bussines
{
    public class BaseManager
    {
        protected readonly bool FUNCTION_APP = true;

        protected ILogger _log;
        public BaseManager(ILogger log)
        {
            _log = log;
        }

        protected IApiResult HandleException(ResultException ex, string message = null)
        {
            message = $"{message ?? "Error"} : {ex.StatusCode} . Exception: {ex.Message}";
            _log.LogError(ex, message);

            return new ApiResult().SetError(message, ex.StatusCode);
        }

        protected IApiResult HandleException(Exception ex, HttpStatusCode statusCode, string message = null, LogLevel level = LogLevel.Error)
        {
            message = $"{message ?? "Error"} : {statusCode} . Exception: {ex.Message}";
            _log.Log(level, ex, message);

            return new ApiResult().SetError(message, statusCode);
        }

        protected IApiResult HandleSuccess(string message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            message = $"{message ?? "Operacion Realizada con exito."} - {statusCode}";
            _log.LogInformation(message);

            return new ApiResult(message, statusCode);
        }

        private string GetMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;

            return ex.Message;
        }
    }
}
