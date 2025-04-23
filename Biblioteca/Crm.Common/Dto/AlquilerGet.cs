using Newtonsoft.Json;

namespace Crm.Common.Dto
{
    public class AlquilerGet
    {
        [JsonProperty("dn_alquilerid")]
        public Guid Id { get; set; }

        public string Biblioteca { get; set; }

        public string Libro { get; set; }

        public string Socio { get; set; }

        [JsonProperty("dn_name")]
        public string Name { get; set; }

        [JsonProperty("dn_desde")]
        public DateTime Desde { get; set; }

        [JsonProperty("dn_hasta")]
        public DateTime Hasta { get; set; }

        [JsonProperty("_dn_biblioteca_value")]
        public Guid BibliotecaGuid { get; set; }

        [JsonProperty("_dn_libro_value")]
        public Guid LibroGuid { get; set; }

        [JsonProperty("_dn_socio_value")]
        public Guid SocioGuid { get; set; }

    }
}
