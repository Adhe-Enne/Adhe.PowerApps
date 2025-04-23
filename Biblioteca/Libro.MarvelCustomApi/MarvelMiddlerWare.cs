using Axxon.PluginCommons;
using Libro.CustomApi.Dto;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Libro.MarvelCustomApi
{
    public class MarvelMiddlerWare : PluginBase
    {
        const string FUNCTION_URL = "FUNC_MARVEL_URL";
        const string FUNCTION_CODE = "FUNC_MARVEL_CODE";
        public override void Execute(PluginBag bag)
        {
            string nameStartsWith = bag.PluginContext.InputParameters["HeroNameIn"].ToString();
            string responseJson = string.Empty;
            string url = bag.GetEnvironmentVariableValue(FUNCTION_URL);
            string code = bag.GetEnvironmentVariableValue(FUNCTION_CODE);

            if (string.IsNullOrEmpty(url))
                throw new Exception("La URL de la función no está configurada en las variables de entorno.");

            if (string.IsNullOrEmpty(code))
                throw new Exception("El código de credencial de la función no está configurado en las variables de entorno.");

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{url}?code={code}&nameStartsWith={Uri.EscapeDataString(nameStartsWith)}";

                HttpResponseMessage response = client.GetAsync(requestUrl).Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error en la solicitud: {response.StatusCode} - {response.RequestMessage.ToString()}");

                responseJson = response.Content.ReadAsStringAsync().Result;

                response.EnsureSuccessStatusCode();
            }

            MarvelCharacterResponse marvelCharacterResponse = JsonConvert.DeserializeObject<MarvelCharacterResponse>(responseJson);

            if (marvelCharacterResponse.code != 200)
                throw new Exception($"Error en la respuesta de la API: {marvelCharacterResponse.code} {marvelCharacterResponse.status}");

            if (marvelCharacterResponse.data is null)
                throw new Exception("No se encontraron personajes con el nombre proporcionado.");

            if (marvelCharacterResponse.data.results.Count == 0)
                throw new Exception("No se encontraron personajes con el nombre proporcionado.");

            bag.PluginContext.OutputParameters["HeroJsonOut"] = JsonConvert.SerializeObject(marvelCharacterResponse);
        }
    }
}
