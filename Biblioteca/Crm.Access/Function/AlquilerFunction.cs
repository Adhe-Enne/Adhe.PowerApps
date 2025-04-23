using Core.External;
using Core.Framework;
using Core.Framework.Exceptions;
using Crm.Access.FunctionTools;
using Crm.Common.Bussines;
using Crm.Common.Dto;
using Crm.Common.Interfaces;
using Crm.Common.Model;
using Crm.Common.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System.Net;

namespace Crm.Access.Function
{
    public class AlquilerFunction : FunctionCrmBase
    {
        ICrmService _crmService;
        public AlquilerFunction(ILogger<AlquilerFunction> logger, HttpClient httpClient, ICrmService clientService, CrmSettings settings)
            : base(logger, settings)
        {
            _crmService = clientService;
        }

        [Function("Get-Alquiler")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Alquiler")] HttpRequest req, FunctionContext context)
        {
            _log.LogInformation("Consultando registros en Dataverse...");

            ApiResultData<AlquilerGet> result = new();

            try
            {
                IRequestGet query = FunctionHelper.ToParameters(req.Query);
                await _crmService.ChangeDestiny(query.UseExternalUrl);
                var response = await _crmService.GetRecordsHttpClient(query);
                var mapped = EntityMapper.Map<AlquilerGet>(response, _settings.GetMap(query.MapperName));

                result.AddData(mapped);
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

        [Function("Get-Alquiler-Crm")]
        public async Task<IActionResult> ByServiceClient([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Alquiler/Crm")] HttpRequest req)
        {
            _log.LogInformation("Consultando registros en Dataverse...");

            ApiResultData<AlquilerGet> result = new();

            try
            {
                IRequestGet query = FunctionHelper.ToParameters(req.Query);
                await _crmService.ChangeDestiny(query.UseExternalUrl);
                await _crmService.RestartCrmClient();

                EntityCollection response = await _crmService.GetRecords(query);

                List<AlquilerGet> records = EntityMapper.Map<AlquilerGet>(response.Entities.ToList(), _settings.GetMap(query.MapperName));

                result.AddData(records);
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

        [Function("Post-Alquiler-Crm")]
        public async Task<IActionResult> RunByClientDataverse([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Alquiler/Crm")] HttpRequestData req, FunctionContext executionContext)
        {
            _log.LogInformation("Iniciando la creación del alquiler...");

            ApiResult<Guid> result = new();

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = Json.Deserialize<RequestPost<AlquilerPost>>(requestBody);

                await _crmService.ChangeDestiny(data.UseExternalUrl);
                await _crmService.RestartCrmClient();
                result.Data = await _crmService.PostCrm<AlquilerPost>(data);

                result.Set(HandleSuccess($"Alquiler creado con éxito. ID: {result.Data}. ", HttpStatusCode.Created));
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al Crear Alquiler"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico al Crear Alquiler", LogLevel.Critical));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }

        [Function("Post-Alquiler")]

        public async Task<IActionResult> Savealquiler([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Alquiler")] HttpRequestData req)
        {
            _log.LogInformation("Iniciando la creación del Alquiler...");
            ApiResult<string> result = new();

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var reqContent = Json.Deserialize<RequestPost<AlquilerPost>>(requestBody);
                _crmService.ChangeDestiny(reqContent.UseExternalUrl);

                Guid rentalId = await _crmService.PostHttpClient<AlquilerPost>(reqContent);

                result.Set(HandleSuccess($"Alquiler creado con exito. Guid: {rentalId}. ", HttpStatusCode.Created));
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al Crear Alquiler"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico al Crear Alquiler", LogLevel.Critical));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }
    }
}
