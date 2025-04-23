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
    public class SocioFunction : FunctionCrmBase
    {
        ICrmService _crmService;
        public SocioFunction(ILogger<AlquilerFunction> logger, HttpClient httpClient, ICrmService clientService, CrmSettings settings)
            : base(logger, settings)
        {
            _crmService = clientService;
        }

        [Function("Post-Socio")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Socio")] HttpRequestData req, FunctionContext executionContext)
        {
            _log.LogInformation("Procesando solicitud para crear alquiler en Dataverse...");

            ApiResult result = new();

            try
            {
                var reqData = await new StreamReader(req.Body).ReadToEndAsync();
                var reqPost = Json.Deserialize<RequestPost<SocioPost>>(reqData);
                await _crmService.ChangeDestiny(reqPost.UseExternalUrl);

                Guid token = await _crmService.PostHttpClient<SocioPost>(reqPost);

                result.Set(HandleSuccess($"Socio creado con exito. Guid: {token}. ", HttpStatusCode.Created));
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al consultar Crear Socio. "));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico", LogLevel.Critical));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }

        [Function("Post-Socio-Crm")]
        public async Task<IActionResult> ByServiceClient([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Socio/Crm")] HttpRequest req)
        {
            _log.LogInformation("Iniciando la creación del Socio...");

            ApiResult result = new();

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = Json.Deserialize<RequestPost<SocioPost>>(requestBody);
                await _crmService.ChangeDestiny(data.UseExternalUrl);
                await _crmService.RestartCrmClient();
                Guid rentalId = await _crmService.PostCrm<SocioPost>(data);

                result.Set(HandleSuccess($"Socio creado con éxito. ID: {rentalId}. ", HttpStatusCode.Created));
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al Crear Socio"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico al Crear Socio", LogLevel.Critical));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }

        [Function("Get-Socio-BibliotecaLibro")]
        public async Task<IActionResult> GetAvailable([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Socio/BibliotecaLibro")] HttpRequest req, FunctionContext context)
        {
            _log.LogInformation("Consultando Stock de Libros por Biblioteca en Dataverse...");

            ApiResultCrmData<BibliotecaLibroGet> result = new();

            try
            {
                IRequestGet query = FunctionHelper.ToParameters(req.Query);
                await _crmService.ChangeDestiny(query.UseExternalUrl);
                await _crmService.RestartCrmClient();

                EntityCollection response = await _crmService.GetRecords(query);
                List<BibliotecaLibroGet> mapped = EntityMapper.Map<BibliotecaLibroGet>(response.Entities.ToList(), _settings.Dicctionary(query.MapperName));

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

        [Function("Get-Socio-Crm")]
        public async Task<IActionResult> GetSocio([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Socio/Crm")] HttpRequest req)
        {
            _log.LogInformation("Procesando solicitud para crear alquiler en Dataverse...");

            ApiResultCrmData<SocioGet> result = new();

            try
            {
                IRequestGet query = FunctionHelper.ToParameters(req.Query);
                await _crmService.ChangeDestiny(query.UseExternalUrl);
                await _crmService.RestartCrmClient();

                var response = await _crmService.GetRecordsCrm<SocioGet>(query);
                result.AddData(response);
                result.Set(HandleSuccess($"Consulta de Socio a dataverse exitoso. Resultados: {response.Count}"));
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al consultar Crear Socio. "));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico", LogLevel.Critical));
            }

            return new ObjectResult(result) { StatusCode = (int) result.StatusCode };
        }
    }
}
