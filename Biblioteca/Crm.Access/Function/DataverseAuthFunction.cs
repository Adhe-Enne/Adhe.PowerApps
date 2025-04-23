using Core.Framework;
using Core.Framework.Bussines;
using Core.Framework.Exceptions;
using Crm.Access.FunctionTools;
using Crm.Common.Interfaces;
using Crm.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Crm.Access.Function
{
    public class DataverseAuthFunction : BaseManager
    {
        private readonly ILogger<DataverseAuthFunction> _logger;
        private readonly ICrmService _envCrmService;

        public DataverseAuthFunction(ILogger<DataverseAuthFunction> logger, ICrmService tokenRequestService) : base(logger)
        {
            _logger = logger;
            _envCrmService = tokenRequestService;
        }

        [Function("Auth")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Auth")] HttpRequest req)
        {
            ApiResult<string> result = new ApiResult<string>();
            _logger.LogInformation("Solicitando token de autenticación para Dataverse...");

            try
            {
                IRequestData query = FunctionHelper.ToParameters(req.Query);
                _envCrmService.ChangeDestiny(query.UseExternalUrl);

                result.Data = await _envCrmService.GetCrmToken();
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al autenticar en Dataverse"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico"));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }

        [Function("Auth-Crm")]
        public async Task<IActionResult> ByServiceClient([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Auth/Crm")] HttpRequest req)
        {
            _logger.LogInformation("Solicitando token de autenticación para Dataverse...");

            ApiResult<string> result = new();

            try
            {
                IRequestData query = FunctionHelper.ToParameters(req.Query);
                await _envCrmService.ChangeDestiny(query.UseExternalUrl);
                await _envCrmService.RestartCrmClient();

                result.Data = await _envCrmService.GetHttpToken();

                _logger.LogInformation("Token obtenido correctamente.");
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al consultar informacion del dataverse. "));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico"));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }
    }
}
