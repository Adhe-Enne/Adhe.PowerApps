namespace Core.Model
{
    public class AppSettings
    {
        public bool IS_FUNCTION_APP { get; set; }
        public string TENANT_ID { get; set; } = string.Empty;
        public string CLIENT_ID { get; set; } = string.Empty;
        public string CLIENT_SECRET { get; set; } = string.Empty;
        public string RESOURCE_URL { get; set; } = string.Empty;
    }
}
