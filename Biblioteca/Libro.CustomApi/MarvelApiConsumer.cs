using Libro.CustomApi.Dto;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Libro.CustomApi
{
    public class MarvelApiConsumer : IPlugin
    {
        private const string MarvelApiBaseUrl = "https://gateway.marvel.com/v1/public/";
        private const string HASH = "MARVEL_HASH_CONST";
        private const string PUBLIC_APIKEY = "MARVEL_APIKEY_PUBLIC";
        private const string TIMESTAMP = "MARVEL_TIMESTAMP";
        private const bool LIMIT_20 = true;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService) serviceProvider.GetService(typeof(ITracingService));

            try
            {
                string hash = GetEnvironmentVariableValue(service, HASH);
                string publicAp = GetEnvironmentVariableValue(service, PUBLIC_APIKEY);
                string timeStamp = GetEnvironmentVariableValue(service, TIMESTAMP);

                if (string.IsNullOrEmpty(publicAp) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(timeStamp))
                    throw new InvalidPluginExecutionException("Las claves de API no están configuradas en las variables de entorno.");

                // Obtener nombre de personaje desde la solicitud (si se envía)
                string characterName = context.InputParameters.Contains("LibroCharacterName") ? context.InputParameters["LibroCharacterName"].ToString() : "";

                tracingService.Trace($"Buscando personaje: {characterName}");

                // Llamar a la API de Marvel
                MarvelCharacterResponse response = Task.Run(() => GetMarvelCharacter(characterName, publicAp, hash, timeStamp, tracingService)).Result;


                if (response.code != 200)
                {
                    throw new InvalidPluginExecutionException($"Error en la respuesta de la API: {response.status}");
                }   

                if (response.data.count == 0)
                {
                    throw new InvalidPluginExecutionException("No se encontraron personajes con el nombre proporcionado.");
                }

                if (response.data.results.Count ==0)
                {
                    throw new InvalidPluginExecutionException("No se encontraron personajes con el nombre proporcionado.");
                }

                // Retornar la respuesta JSON en el parámetro de salida
                context.OutputParameters["MarvelApiResponse"] = Newtonsoft.Json.JsonConvert.SerializeObject(response.data.results);

                tracingService.Trace("Consumo de API Marvel exitoso");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error en MarvelApiPlugin: {ex.Message}");
                throw new InvalidPluginExecutionException($"Error en el plugin: {ex.Message}");
            }
        }

        private async Task<MarvelCharacterResponse> GetMarvelCharacter(string characterName, string publicKey, string hash, string timeStamp, ITracingService tracingService)
        {
            using (HttpClient client = new HttpClient())
            {
                // Construir URL de la petición
                string url = $"{MarvelApiBaseUrl}characters?apikey={publicKey}&ts={timeStamp}&hash={hash}";

                if (!string.IsNullOrEmpty(characterName))
                {
                    url += $"&name={Uri.EscapeDataString(characterName)}";
                }

                if (LIMIT_20)
                {
                    url += "&limit=20";
                }

                tracingService.Trace($"URL de solicitud: {url}");

                // Realizar solicitud HTTP
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode != true)
                {
                    throw new Exception("ERROR en httpRequest");
                }

                string content = response.Content.ReadAsStringAsync().Result;

                tracingService.Trace("http Request OK");

                response.EnsureSuccessStatusCode();

                MarvelCharacterResponse apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<MarvelCharacterResponse>(content);

                return apiResponse;
            }
        }

        private string GenerateMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return string.Concat(hashBytes.Select(b => b.ToString("x2")));
            }
        }

        private string GetEnvironmentVariableValue(IOrganizationService service, string variableName)
        {
            //filtramos en forma descendente y tomamos el primero
            string fetchXml = $@"
                <fetch top='1'>
	                <entity name='environmentvariabledefinition'>
		                <filter type='and'>
			                <condition attribute='displayname' operator= 'eq' value='{variableName}' />
		                </filter>
	                </entity>
                </fetch>";

            EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Count == 0) return null;

            Entity entity = results.Entities.FirstOrDefault();

            if (!entity.Attributes.ContainsKey("defaultvalue")) return null;

            return entity.Attributes["defaultvalue"].ToString();
        }
    }
}
