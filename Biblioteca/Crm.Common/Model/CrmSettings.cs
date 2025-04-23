using Core.External;
using Core.Model;
using Microsoft.Crm.Sdk.Messages;
using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Crm.Common.Model
{
    public class CrmSettings : AppSettings
    {
        public Dictionary<string, string> Variables { get; set; }

        public string EXTERNAL_RESOURCE_URL { get; set; } = string.Empty;
        public Dictionary<string, string> EntityMappers { get; set; } = new();
        public Dictionary<string, string> ALQ_GET_ENT { get; set; } = new();
        public Dictionary<string, string> MG_BIBLIB_GET_ENT { get; set; } = new();
        public Dictionary<string, string> ALQ_GET_JSON { get; set; } = new();
        public Dictionary<string, string> ALQ_POST_HTTP { get; set; } = new();
        public Dictionary<string, MappingRule> ALQ_POST_CRM { get; set; } = new();
        public Dictionary<string, string> MG_ALQ_GET_JSON { get; set; } = new();
        public Dictionary<string, string> MG_ALQ_GET_ENT { get; set; } = new();
        public Dictionary<string, string> MG_SOC_POST_HTTP { get; set; } = new();
        public Dictionary<string, MappingRule> MG_SOC_POST_CRM { get; set; } = new();

        public Dictionary<string, string> GetMap(string environmentVariableName)
        {
            var value = this.Variables[environmentVariableName];

            if (string.IsNullOrEmpty(value))
                throw new Exception($"Clave '{environmentVariableName}' no existe en las variables de Entorno.");

            var atribute =  Json.ToDictionary<string, string>(value);

            if (atribute.Any())
                return atribute;

            var property = this.GetType().GetProperty(environmentVariableName);

            if (property == null)
            {
                throw new Exception($"La propiedad '{environmentVariableName}' no existe en _settings.");
            }

            var dictionary = property.GetValue(this) as Dictionary<string, string>;

            if (dictionary == null)
            {
                throw new Exception($"La propiedad '{environmentVariableName}' no es un diccionario.");
            }

            return dictionary;
        }

        public Dictionary<string, MappingRule> GetMapRule(string environmentVariableName)
        {
            var value = Variables[environmentVariableName];

            if (string.IsNullOrEmpty(value))
                throw new Exception($"Clave '{environmentVariableName}' no existe en las variables de Entorno.");

            var mapRule = Json.ToDictionary<string, MappingRule>(value);

            if (mapRule.Any())
                return mapRule;

            var property = this.GetType().GetProperty(environmentVariableName);

            if (property == null)
            {
                throw new Exception($"La propiedad '{environmentVariableName}' no existe en variables de Entorno.");
            }

            var dictionary = property.GetValue(this) as Dictionary<string, MappingRule>;

            if (dictionary == null)
            {
                throw new Exception($"La propiedad '{environmentVariableName}' no es un diccionario.");
            }

            return dictionary;
        }

        public Dictionary<string, string> Dicctionary(string environmentVariableName)
        {
            var value = Variables[environmentVariableName];

            if (string.IsNullOrEmpty(value))
                throw new Exception($"Clave '{environmentVariableName}' no existe en las variables de Entorno.");

            return Json.ToDictionary<string, string>(value);
        }

        public static CrmSettings LoadFromEnvironment()
        {

            return new CrmSettings
            {
                Variables = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(kvp => (string) kvp.Key, kvp => (string) kvp.Value),

                TENANT_ID = GetEnv("TENANT_ID"),
                CLIENT_ID = GetEnv("CLIENT_ID"),
                CLIENT_SECRET = GetEnv("CLIENT_SECRET"),
                RESOURCE_URL = GetEnv("RESOURCE_URL"),
                EXTERNAL_RESOURCE_URL = GetEnv("EXTERNAL_RESOURCE_URL"),

                EntityMappers = Json.ToDictionary(GetEnv("ENTITY_MAPPERS")),
                ALQ_GET_ENT = Json.ToDictionary(GetEnv("ALQ_GET_ENT")),
                ALQ_GET_JSON = Json.ToDictionary(GetEnv("ALQ_GET_JSON")),
                ALQ_POST_HTTP = Json.ToDictionary(GetEnv("ALQ_POST_HTTP")),
                MG_BIBLIB_GET_ENT = Json.ToDictionary(GetEnv("MG_BIBLIB_GET_ENT")),
                ALQ_POST_CRM = Json.ToDictionary<string, MappingRule>(GetEnv("ALQ_POST_CRM")),

                MG_ALQ_GET_JSON = Json.ToDictionary(GetEnv("MG_ALQ_GET_JSON")),
                MG_ALQ_GET_ENT = Json.ToDictionary(GetEnv("MG_ALQ_GET_ENT")),
                MG_SOC_POST_HTTP = Json.ToDictionary(GetEnv("MG_SOC_POST_HTTP")),
                MG_SOC_POST_CRM = Json.ToDictionary<string, MappingRule>(GetEnv("MG_SOC_POST_CRM")),
            };
        }

        private static string GetEnv(string key) => Environment.GetEnvironmentVariable(key) ?? string.Empty;
    }
}
