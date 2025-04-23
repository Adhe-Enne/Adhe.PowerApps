namespace Core.Model
{
    public class CrmConnectionSettings
    {
        public string TENANT_ID { get; set; }
        public string CLIENT_ID { get; set; }
        public string CLIENT_SECRET { get; set; }
        public string RESOURCE_URL { get; set; }

        //OPTIONAL
        public string EXTERNAL_RESOURCE_URL { get; set; }

        public bool CheckSettings()
        {
            return string.IsNullOrEmpty(TENANT_ID) && string.IsNullOrEmpty(CLIENT_ID) && string.IsNullOrEmpty(CLIENT_SECRET) && string.IsNullOrEmpty(RESOURCE_URL);
        }
    }
}
