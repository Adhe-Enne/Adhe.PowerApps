namespace Socio.Api.Interfaces
{
    public interface IRequestService
    {
        // Define the methods that TokenRequestService should implement
        public Dictionary<string, string> CreateTokenRequestValues(string clientId, string clientSecret, string dataverseUrl);
        public Task<string> GetAccessToken();
        public Task<string> CrmStringUrl();
        public Task<Model.Credential> GetCredentials(bool fromVault = false);

    }
}