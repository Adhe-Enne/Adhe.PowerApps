using Core.Model;
using Crm.Common.Dto;
using Crm.Common.Interfaces;
using Crm.Common.Model;
using Crm.Common.Tools;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Crm.Common.Services
{
    public class CrmService : ICrmService
    {
        private readonly HttpClient _httpClient;
        private readonly CrmConnectionSettings _cfg;
        private CrmConnector _connector;
        private Credential _cred;
        private readonly CrmSettings _settings;
        public CrmService(HttpClient httpClient, CrmSettings settings)
        {
            _httpClient = httpClient;
            _settings = settings;
            _cred = GetCredentials().Result;
            _connector = new CrmConnector(_cred);
        }

        public async Task<string> GetHttpToken()
        {
            return await _connector.GetAuthHttp();
        }

        public async Task<string> GetCrmToken()
        {
            return await _connector.GetAuthClient();
        }

        public async Task<EntityCollection> GetRecords(IRequestGet query)
        {
            _connector.setParameters(query);

            return await _connector.GetCrmEntities();
        }

        public async Task<string> GetRecordsHttpClient(IRequestGet query)
        {
            _connector.setParameters(query);
            return await _connector.GetJsonEntities();
        }

        public async Task<List<T>> GetRecordsHttpClient<T>(IRequestGet query) where T : class, new()
        {
            _connector.setParameters(query);
            var response = await _connector.GetJsonEntities();

            return EntityMapper.Map<T>(response, _settings.GetMap(query.MapperName)); ;
        }

        public async Task<List<T>> GetRecordsCrm<T>(IRequestGet query) where T : class, new()
        {
            _connector.setParameters(query);
            var response = await _connector.GetCrmEntities();

            List<T> records = EntityMapper.Map<T>(response.Entities.ToList(), _settings.GetMap(query.MapperName));

            return records;
        }

        public async Task<Guid> PostHttpClient<T>(IRequestData<T> reqContent) where T : class, new()
        {
            var mapper = EntityMapper.ToDiccionary<T>(reqContent.Body, _settings.GetMap(reqContent.MapperName));

            return await _connector.SaveRecordHttp(mapper, reqContent.EntityName);
        }

        public async Task<Guid> PostCrm<T>(IRequestData<T> reqContent) where T : class, new()
        {
            var entity = EntityMapper.Map<T>(reqContent.Body, reqContent.EntityName, _settings.GetMapRule(reqContent.MapperName));

            return await _connector.SaveRecordCrm(entity);
        }

        public Credential GetCredential(bool UseExternalDataverse = false)
        {
            _cred.UseExternalUrl = UseExternalDataverse;
            return _cred;
        }

        public async Task<Credential> GetCredentials()
        {
            Credential cred = new(_settings.RESOURCE_URL, _settings.EXTERNAL_RESOURCE_URL);
            cred.TenantId = _settings.TENANT_ID;
            cred.ClientId = _settings.CLIENT_ID;
            cred.ClientSecret = _settings.CLIENT_SECRET;

            if (string.IsNullOrEmpty(cred.TenantId) && string.IsNullOrEmpty(cred.ClientId) && string.IsNullOrEmpty(cred.ClientSecret) && string.IsNullOrEmpty(cred.ResourceUrl))
                throw new Exception("Credenciales de conexion a CRM incompletas");

            return cred;
        }

        public async Task ChangeDestiny(bool UseExternalDataverse = false)
        {
            _cred.UseExternalUrl = UseExternalDataverse;
        }

        public async Task RestartCrmClient()
        {
            Console.WriteLine("Antes de reiniciar: " + (_connector == null ? "Conector es NULL" : "Conector tiene instancia"));

            if (_connector != null)
            {
                _connector.Dispose(); // Si CrmConnector implementa IDisposable
                _connector = null;
            }

            _connector = new CrmConnector(_cred);

            Console.WriteLine("Después de reiniciar: " + (_connector == null ? "Conector sigue NULL" : "Nuevo conector creado"));

            await _connector.ServiceCheckConnection();
        }


    }
}
