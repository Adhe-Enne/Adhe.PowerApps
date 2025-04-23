using Socio.Api.Interfaces;
using Socio.Api.Model;
using System.Text.Json;

namespace Socio.Api.Services
{
    public class RequestService : IRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly CrmConnectionSettings _cfg;

        public RequestService(HttpClient httpClient, IConfiguration cfg)
        {
            _httpClient = httpClient;
            _cfg = cfg.GetSection("CrmConnection").Get<CrmConnectionSettings>();
        }

        public Dictionary<string, string> CreateTokenRequestValues(string clientId, string clientSecret, string dataverseUrl)
        {
            return new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "scope", $"{dataverseUrl}/.default" },
                { "grant_type", "client_credentials" }
            };
        }

        public async Task<string> GetAccessToken()
        {
            string? tenantId = _cfg.TENANT_ID;// Environment.GetEnvironmentVariable(Constants.TENANT_ID);
            string? clientId = _cfg.CLIENT_ID; //Environment.GetEnvironmentVariable(Constants.CLIENT_ID);
            string? clientSecret = _cfg.CLIENT_SECRET; //Environment.GetEnvironmentVariable(Constants.CLIENT_SECRET);
            string? dataverseUrl = _cfg.RESOURCE_URL; //Environment.GetEnvironmentVariable(Constants.RESOURCE_URL);

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) ||
                string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(dataverseUrl))
            {
                throw new Exception("Error: Configuración incompleta en las variables de entorno");
            }

            string tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var values = this.CreateTokenRequestValues(clientId, clientSecret, dataverseUrl);

            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(values));
            if (!response.IsSuccessStatusCode) return null;

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            return jsonResponse.GetProperty("access_token").GetString();
        }

        public async Task<string> CrmStringUrl()
        {
            string? clientId = Environment.GetEnvironmentVariable(Constants.CLIENT_ID);
            string? clientSecret = Environment.GetEnvironmentVariable(Constants.CLIENT_SECRET);
            string? dataverseUrl = Environment.GetEnvironmentVariable(Constants.RESOURCE_URL);

            return $"AuthType=ClientSecret;Url={dataverseUrl};ClientId={clientId};ClientSecret={clientSecret}";
        }

        public async Task<Model.Credential> GetCredentials(bool fromVault = false)
        {
            /* if (fromVault)
             {
                 return null; //await CredentialsByVault();
             }*/

            return CredentialsByEnv();
        }
        /*
        private async Task<Model.Credential> CredentialsByVault()
        {
            string? keyVaultUrl = Environment.GetEnvironmentVariable(Constants.KEYVAULT_URL);

            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new Exception("Error: No se encontró la URL de Key Vault en las variables de entorno");
            }

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            Model.Credential cred = new();
            // 🔹 Obtener valores secretos desde Key Vault
            cred.TenantId = (await client.GetSecretAsync(Constants.TENANT_ID)).Value.Value;
            cred.ClientId = (await client.GetSecretAsync(Constants.CLIENT_ID)).Value.Value;
            cred.ClientSecret = (await client.GetSecretAsync(Constants.CLIENT_SECRET)).Value.Value;
            cred.DataverseUrl = (await client.GetSecretAsync(Constants.RESOURCE_URL)).Value.Value;

            return cred;
        }*/

        private Model.Credential CredentialsByEnv()
        {
            string? tenantId = _cfg.TENANT_ID;// Environment.GetEnvironmentVariable(Constants.TENANT_ID);
            string? clientId = _cfg.CLIENT_ID; //Environment.GetEnvironmentVariable(Constants.CLIENT_ID);
            string? clientSecret = _cfg.CLIENT_SECRET; //Environment.GetEnvironmentVariable(Constants.CLIENT_SECRET);
            string? dataverseUrl = _cfg.RESOURCE_URL; //Environment.GetEnvironmentVariable(Constants.RESOURCE_URL);

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(dataverseUrl))
            {
                throw new Exception("Error: Configuración incompleta en las variables de entorno");
            }

            Model.Credential cred = new()
            {
                TenantId = tenantId,
                ClientId = clientId,
                ClientSecret = clientSecret,
                DataverseUrl = dataverseUrl
            };

            return cred;
        }
    }
}