using Socio.Api.Model;

namespace Socio.Api.Services
{
    public class SocioService
    {
        CrmConnectionSettings _settings;
        public SocioService(IConfiguration cfg)
        {
            _settings = (CrmConnectionSettings?) cfg.GetSection("CrmConnection");
        }
    }
}
