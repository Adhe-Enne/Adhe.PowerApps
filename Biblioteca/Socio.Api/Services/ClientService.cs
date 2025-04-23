using Crm.Access.Exceptions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using MscrmTools.FluentQueryExpressions;
using Socio.Api.Interfaces;
using Socio.Api.Model;
using Socio.Api.Tools;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Socio.Api.Services
{
    public class ClientService : IClientService
    {
        private readonly CrmConnectionSettings _cfg;
        ServiceClient _client;
        IRequestService _requestService;

        public ClientService(IConfiguration cfg, IRequestService requestService)
        {
            _cfg = cfg.GetSection("CrmConnection").Get<CrmConnectionSettings>();
            _requestService = requestService;
        }

        public EntityCollection GetRecords(ParametersQuery query)
        {
            CheckConecction();

            string fetchXml = OdataHelper.BuildFetchXml(query, "dn_alquiler");
            EntityCollection data = _client.RetrieveMultipleAsync(new Microsoft.Xrm.Sdk.Query.FetchExpression(fetchXml)).Result;

            return data;
        }

        public void UpdateCustomer(Guid socioId)
        {
            CheckConecction();

            ColumnSet columns = new ColumnSet(true);
            Entity result = _client.RetrieveAsync("dn_socio", socioId, columns).Result;

            if (result is null)
                throw new ResultException($"Socio no encontrado: " + _client.LastError, HttpStatusCode.NotFound);

            if (!result.Attributes.Any(x => x.Key == "statuscode" || x.Key == "statecode"))
                throw new ResultException($"La respuesta no tiene campos clave: " + _client.LastError, HttpStatusCode.NotFound);

            int stateCode = result.GetAttributeValue<OptionSetValue>("statecode").Value; // State
            int statusCode = result.GetAttributeValue<OptionSetValue>("statuscode").Value; // State

            if (stateCode == 1 && statusCode == 2)
                throw new ResultException($"El Socio {socioId} ya se encuentra inactivo. " + _client.LastError, HttpStatusCode.Conflict);

            Entity record = new Entity("dn_socio", socioId);
            record["statecode"] = new OptionSetValue(1); // State
            record["statuscode"] = new OptionSetValue(2); // Status

            _client.Update(record);
        }

        public void DisableRentals(Guid socioId)
        {
            CheckConecction();

            Query<Entity> query = new Query("dn_alquiler")
                .Select(true)
                .WhereEqual("dn_socio", socioId);

            EntityCollection alquilers = _client.RetrieveMultiple(query);

            if (alquilers is null)
                throw new ResultException($"La respuesta del Dataverse fue nula con el Socio {socioId}. " + _client.LastError, HttpStatusCode.NotFound);

            if (alquilers.Entities.Count == 0)
                throw new ResultException($"No se encontraron alquileres para el Socio {socioId}. " + _client.LastError, HttpStatusCode.NotFound);

            List<Entity> alquileres = alquilers.Entities.ToList();

            ExecuteMultipleRequest executeMultipleRequest = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                }
            };

            foreach (var alquiler in alquileres)
            {
                SetStateRequest setStateRequest = new SetStateRequest
                {
                    EntityMoniker = new EntityReference("dn_alquiler", alquiler.Id),
                    State = new OptionSetValue(1),  // Inactivo
                    Status = new OptionSetValue(2)  // Código de estado (puede variar)
                };

                executeMultipleRequest.Requests.Add(setStateRequest);
            }

            _client.Execute(executeMultipleRequest);
        }

        public async void UpdateRental(Guid socioId)
        {
            CheckConecction();

            ColumnSet columns = new ColumnSet(true);
            Entity result = await _client.RetrieveAsync("dn_socio", socioId, columns);
            // Columns

            if (result is null)
                throw new ResultException($"Socio no encontrado: " + _client.LastError, HttpStatusCode.NotFound);

            if (!result.Attributes.Any(x => x.Key == "statuscode" || x.Key == "statecode"))
                throw new ResultException($"La respuesta no tiene campos clave: " + _client.LastError, HttpStatusCode.NotFound);

            int stateCode = result.GetAttributeValue<OptionSetValue>("statecode").Value; // State
            int statusCode = result.GetAttributeValue<OptionSetValue>("statuscode").Value; // State

            if (stateCode == 1 && statusCode == 2)
                throw new ResultException($"El Socio {socioId} ya se encuentra inactivo. " + _client.LastError, HttpStatusCode.Conflict);

            Entity record = new Entity("dn_socio", new Guid("00000000-0000-0000-0000-000000000000"));
            record["statecode"] = new OptionSetValue(1); // State
            record["statuscode"] = new OptionSetValue(2); // Status

            await _client.UpdateAsync(record);
        }

        public async Task<string> GetRecordsHttpClient(ParametersQuery query)
        {
            Credential credential = await _requestService.GetCredentials();
            string token = await _requestService.GetAccessToken();
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestUrl = $"{credential.DataverseUrl}/api/data/v9.2/dn_alquilers";

            // if (!string.IsNullOrEmpty(query.Filter))
            requestUrl += OdataHelper.BuildFilterUrl(query);

            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error en la consulta a Dataverse: {response.ReasonPhrase}");
            }

            var readResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            var data = readResponse.GetProperty("value").ToString();

            return data;
        }

        private void CheckConecction()
        {
            if (_cfg.CheckSettings())
                throw new ResultException("Error Configuracion de Conexion a CRM, verifique valores.", HttpStatusCode.InternalServerError);

            _client = new ServiceClient(new Uri(_cfg.RESOURCE_URL), _cfg.CLIENT_ID, _cfg.CLIENT_SECRET, false);

            if (!_client.IsReady)
                throw new ResultException($"No se pudo conectar a Dataverse: " + _client.LastError, HttpStatusCode.BadGateway);
        }

    }
}
