using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socio.Api.Model
{
    public class Credential
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DataverseUrl { get; set; }
        public string TenantId { get; set; }

    }
}
