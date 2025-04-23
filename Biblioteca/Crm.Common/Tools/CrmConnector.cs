using Core.External;
using Core.Framework.Exceptions;
using Core.Model;
using Crm.Common.Dto;
using Crm.Common.Interfaces;
using Crm.Common.Model;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Data.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using MscrmTools.FluentQueryExpressions;
namespace Crm.Common.Tools
{
    public class CrmConnector: IDisposable
    {
        private HttpClient _httpClient = null;
        private ServiceClient _serviceClient = null;
        private readonly Credential _cred = null;
        private IRequestGet _params;

        public CrmConnector(Model.Credential credential, IRequestGet queryParameters) : this(credential)
        {
            _params = queryParameters;
        }

        public CrmConnector(Model.Credential credential)
        {
            _cred = credential;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
            _serviceClient?.Dispose(); // Liberar instancia anterior si existía
            _serviceClient = new ServiceClient(new Uri(_cred.ResourceUrl), _cred.ClientId, _cred.ClientSecret, false);
            ServiceCheckConnection();
        }

        public async Task<string> GetAuthHttp()
        {
            if (_httpClient is null)
            {
                throw new ResultException("Error: HttpClient no inicializado", System.Net.HttpStatusCode.InternalServerError);
            }

            string tokenUrl = $"https://login.microsoftonline.com/{_cred.TenantId}/oauth2/v2.0/token";
            var response = await _httpClient.PostAsync(tokenUrl, _cred.HttpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ResultException($"Error en la consulta Http - {response.RequestMessage}", response.StatusCode);
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            return jsonResponse.GetProperty(Constants.CrmKey.ACC_TOKEN).GetString();
        }

        public async Task<string> GetAuthClient()
        {
            ServiceCheckConnection();
            return _serviceClient.CurrentAccessToken;
        }

        public async Task<EntityCollection> GetCrmEntities()
        {
            ServiceCheckConnection();
            string fetchXml = OdataHelper.BuildFetchXml(_params);

            return await _serviceClient.RetrieveMultipleAsync(new Microsoft.Xrm.Sdk.Query.FetchExpression(fetchXml));
        }

        public async Task<Entity> GetCrmEntity(string entityName, Guid guid)
        {
            ServiceCheckConnection();

            return await _serviceClient.RetrieveAsync(entityName, guid, new ColumnSet(true));
        }

        public async Task<string> GetJsonEntities()
        {
            string token = await GetAuthHttp();

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

            string resource = _cred.ResourceUrl;
            string requestUrl = $"{resource}/api/data/v9.2/{_params.EntityName}?{OdataHelper.BuildFilterUrl(_params)}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new ResultException($"Error en la consulta a Dataverse - {response.RequestMessage}", response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
            var readResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            return readResponse.GetProperty("value").ToString();
        }

        public async Task<Guid> SaveRecordHttp(Dictionary<string, string> parametersBody, string entityName)
        {
            if (!parametersBody.Any())
            {
                throw new ResultException("Se detecto un Body vacio.", HttpStatusCode.BadRequest);
            }

            string token = await GetAuthClient();

            string postUrl = $"{_cred.ResourceUrl}/api/data/v9.2/{entityName}";  // Modifica con la entidad real

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = Json.Serialize(parametersBody);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(postUrl, jsonContent);

            await ValidateErrorResponse(response);

            string guid = OdataHelper.GetGuidOdata(response);

            return new Guid(guid);
        }

        public async Task<Guid> SaveRecordCrm(Entity entity)
        {
            ServiceCheckConnection();

            if (entity is null)
                throw new ResultException("Se detecto una Entity Crm nula.", HttpStatusCode.BadRequest);

            if (!entity.Attributes.Any())
                throw new ResultException("Se detecto Entity Crm sin atributos.", HttpStatusCode.BadRequest);

            //Capturo error fruta de dataverse
            try
            {
                return await _serviceClient.CreateAsync(entity);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new Exception(ex.Detail.InnerFault.InnerFault.Message);
            }
        }

        public void setParameters(IRequestGet parameter)
        {
            _params = parameter;
        }

        public async Task ServiceCheckConnection()
        {
            if (_serviceClient is null)
            {
                throw new ResultException("ServiceClient no inicializado", System.Net.HttpStatusCode.InternalServerError);
            }

            if (!_serviceClient.IsReady)
                throw new ResultException($"No se pudo conectar a Dataverse: " + _serviceClient.LastError, HttpStatusCode.BadGateway);
        }

        private async Task ValidateErrorResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string responseBody = await response.Content.ReadAsStringAsync();
            string body = await response.RequestMessage.Content.ReadAsStringAsync();

            var errorObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseBody);
            string errorMessage = errorObj?.error?.message ?? "Error desconocido.";

            throw new ResultException($"BodyRequest ----> {body} \nDataverse Error ---->{errorMessage}", HttpStatusCode.BadRequest);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceClient.Dispose();
            }
        }

        ~CrmConnector()
        {
            Dispose(false);
        }
    }
}
