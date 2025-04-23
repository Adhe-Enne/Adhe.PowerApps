namespace Crm.Common.Model
{
    public class Credential
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        private string _localResourceUrl { get; set; }
        private string? _externalDataverseResource { get; set; }
        public bool UseExternalUrl { get; set; }

        public Credential(string localResource, string externalDataverseResource)
        {
            this._localResourceUrl = localResource;
            this._externalDataverseResource = externalDataverseResource;
            this.UseExternalUrl = false;
        }

        public string ResourceUrl =>
            this.UseExternalUrl
                ? (this._externalDataverseResource ?? throw new Exception("No se ha configurado la URL en las credenciales"))
                : this._localResourceUrl;

        public FormUrlEncodedContent HttpContent =>
             new FormUrlEncodedContent( new Dictionary<string, string >{
                { "client_id", this.ClientId },
                { "client_secret", this.ClientSecret },
                { "scope", $"{this.ResourceUrl}/.default" },
                { "grant_type", "client_credentials" }
            });

        public string CrmStringUrl(bool useExternalUrl = false) => $"AuthType=ClientSecret;Url={this.ResourceUrl};ClientId={this.ClientId};ClientSecret={this.ClientSecret}";
    }
}
