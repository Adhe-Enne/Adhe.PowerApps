using AutoMapper;
using Crm.Access.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Socio.Api.Interfaces;
using Socio.Api.Model;
using Socio.Api.Model.Dto;
using Socio.Api.Result;
using System.Net;

namespace Socio.Api.Controllers
{
    [Route("api/[controller]")]
    public class SocioController : BaseController
    {
        private readonly IClientService _clientService;

        public SocioController(ILogger<SocioController> logger, IClientService clientService, IMapper mapper) : base(logger, mapper)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public ActionResult<IApiResultData<Alquiler>> GetRentals([FromQuery] ParametersQuery query)
        {
            ApiResultData<Alquiler> result = new();
            _log.LogInformation("Consultando alquileres...");

            try
            {
                var records = _clientService.GetRecords(query);
                var mapped = _mapper.Map<List<Alquiler>>(records.Entities);

                result.AddData(mapped, query);
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al consultar al Dataverse"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico"));
            }

            return StatusCode((int) result.StatusCode, result);
        }

        [HttpGet("Native")]
        public ActionResult<IApiResultData<Alquiler>> GetRentalsHttpClient([FromQuery] ParametersQuery query)
        {
            ApiResultData<Alquiler> result = new();
            _log.LogInformation("Consultando alquileres...");

            try
            {
                var records = _clientService.GetRecordsHttpClient(query).Result;
                //var mapped = _mapper.Map<List<Alquiler>>(records.Entities);

                result.AddData(JsonConvert.DeserializeObject<List<Alquiler>>(records));
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al consultar al Dataverse"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico"));
            }

            return StatusCode((int) result.StatusCode, result);
        }

        //GET: SocioController/Details/5
        [HttpPatch("DisableCustomer")]
        public ActionResult<IApiResult> UpdateRentalAsync([FromQuery] Guid SocioId)
        {
            ApiResult result = new();

            _log.LogInformation("Inactivando Socio...");

            try
            {
                _clientService.UpdateCustomer(SocioId);
                result.Set("Socio inactivado con éxito", HttpStatusCode.Accepted);
                _log.LogInformation(result.Message);
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al inactivar Socio"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico"));
            }

            return StatusCode((int) result.StatusCode, result);
        }

        [HttpDelete("DisableCustomerRentals")]
        public ActionResult<IApiResult> DisableRental([FromQuery] Guid SocioId)
        {
            ApiResult result = new();
            _log.LogInformation("Inactivando Alquileres Socio...");
            try
            {
                _clientService.DisableRentals(SocioId);
                result.Set("Socio inactivado con éxito", HttpStatusCode.Accepted);
                _log.LogInformation(result.Message);
            }
            catch (ResultException ex)
            {
                result.Set(HandleException(ex, "Error al inactivar Alquileres de Socio"));
            }
            catch (Exception ex)
            {
                result.Set(HandleException(ex, HttpStatusCode.InternalServerError, $"Error critico"));
            }

            return StatusCode((int) result.StatusCode, result);
        }
    }
}
