
using Core.Abstractions;
using System.Net;
using System.Text.Json.Serialization;

namespace Core.Framework
{
    public class ApiResult : IApiResult
    {
        const string DEFAULT_MESSAGE = "Successful";
        public bool HasError { get; set; } //main status
        public string? Message { get; set; } // message
        public HttpStatusCode StatusCode { get; set; }

        public ApiResult()// : base(DEFAULT_MESSAGE)
        {
            this.Message = DEFAULT_MESSAGE;
            this.StatusCode = HttpStatusCode.OK;
            this.HasError = false;
        }

        public ApiResult(string message, HttpStatusCode statusCode, bool hasError = false)
        {
            this.Message = message;
            this.StatusCode = statusCode;
            this.HasError = hasError;
        }

        public ApiResult(Exception ex)
        {
            this.HasError = true;
            this.Message = ex.Message;
            this.StatusCode = HttpStatusCode.InternalServerError;
        }

        public void Set(string Message, bool Error = false)
        {
            this.Message = Message;
            this.HasError = Error;
        }

        public void Set(string Message, HttpStatusCode statusCode)
        {
            this.Message = Message;
            this.StatusCode = statusCode;
        }

        public void AppendMessage(string line)
        {
            if (Message == DEFAULT_MESSAGE) Message = string.Empty;

            Message += line + Environment.NewLine;
        }

        public ApiResult SetError(string Message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            Set(Message, true);
            this.StatusCode = statusCode;

            return this;
        }

        public void Set(IApiResult From)
        {
            this.HasError = From.HasError;
            this.Message = From.Message;
            this.StatusCode = From.StatusCode;
        }

        public string StatusDescription => new HttpResponseMessage(StatusCode).ReasonPhrase;

        public bool IsSuccess() => !HasError;

        public bool IsFailure() => HasError;
    }

    public class ApiResult<T> : ApiResult, IApiResult<T>
    {
        [JsonPropertyOrder(6)] // 🔹 Se mostrará primero
        public T Data { get; set; }

        public ApiResult<T> SetErrorResult(string Message)
        {
            Set(Message, true);

            return this;
        }
    }


}
