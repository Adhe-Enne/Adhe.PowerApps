using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Common.Dto
{
    public class RentalPost
    {
        [JsonProperty("dn_Biblioteca@odata.bind")]
        public string dn_Bibliotecaodatabind { get; set; }

        [JsonProperty("dn_Libro@odata.bind")]
        public string dn_Libroodatabind { get; set; }

        [JsonProperty("dn_Socio@odata.bind")]
        public string dn_Sociodatabind { get; set; }

        public string dn_desde { get; set; }
        public string dn_hasta { get; set; }
    }
}
