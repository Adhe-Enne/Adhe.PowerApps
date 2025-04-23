namespace Crm.Common.Constants
{
    public class CrmKey
    {
        //Environments
        public const string TENANT_ID = "TENANT_ID";
        public const string CLIENT_ID = "CLIENT_ID";
        public const string CLIENT_SECRET = "CLIENT_SECRET";
        public const string RESOURCE_URL = "RESOURCE_URL";
        public const string EXTERNAL_RESOURCE_URL = "EXTERNAL_RESOURCE_URL";
        public const string ALQUILER_MAPPER = "OWN_ALQUILER_MAPPER";
        public const string ENTITY_MAPPERS = "ENTITY_MAPPERS";
        public const string AlquilerEntity = "AlquilerEntity";
        public const string AlquilerJson = "AlquilerJson";
        public const string SocioPost = "SocioPost";

        //Endpoints
        public const string TOKEN_URL = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        public const string CREATE_RENTAL_URL = "{0}/api/data/v9.0/dn_alquiler";
        public const string GET_RENTALS_URL = "{0}/api/data/v9.0/dn_alquiler";

        //Entities
        public const string ACCOUNTS = "accounts";
        public const string LIBRO = "dn_libros";
        public const string ALQUILER = "dn_alquilers";
        public const string BIBLIOTECA = "dn_bibliotecas";

        //Functions
        public const string GET_DAT = "GetDataverseToken";
        public const string CREATE_RENTAL = "CreateRental";
        public const string GET_RENTALS = "GetDataverseRecords";

        //request
        public const string REQ_CLIENT= "client_id";
        public const string REQ_SECRT= "client_secret";
        public const string REQ_SCP= "scope";
        public const string REQ_GRANT= "grant_type";
        public const string REQ_CRED= "client_credentials";
        public const string ACC_TOKEN= "access_token";

        public static string KEYVAULT_URL { get; internal set; }
        public static string BY_SERVICE_CLIENT { get; internal set; }
    }
}
