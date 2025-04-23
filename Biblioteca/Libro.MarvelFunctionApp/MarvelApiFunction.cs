using Libro.CustomApi.Dto;
using Libro.MarvelFunctionApp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Libro.MarvelFunctionApp
{
    public static class MarvelApiConsumer
    {
        private const string MARVEL_URL = "https://gateway.marvel.com/v1/public/";
        private const string MARVEL_PUBLIC_KEY = "MARVEL_APIKEY";
        private const string MARVEL_HASH = "MARVEL_HASH";
        private const string MARVEL_TS = "MARVEL_TIMESTAMP";

        [FunctionName("GetMarvelCharacter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Consumiendo Api Marvel Characters.");

            string name = string.Empty;
            string requestBody = string.Empty;
            Model.Request request = new Model.Request();
            MarvelCharacterResponse marvelCharacterResponse = new MarvelCharacterResponse();
            request.LimitRecords = 0;

            try
            {
                if (req.Method == HttpMethods.Get)
                {
                    request.NameStartsWith = req.Query["nameStartsWith"];

                    if (int.TryParse(req.Query["limitRecords"], out int limitRecords)) request.LimitRecords = limitRecords;
                }
                else if (req.Method == HttpMethods.Post)
                {
                    requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    request = JsonConvert.DeserializeObject<Model.Request>(requestBody);
                }

                if (ValidateRequest(request, out string error))
                {
                    return new BadRequestObjectResult(error);
                }

                marvelCharacterResponse = await GetMarvelCharacter(request, log);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            return new OkObjectResult(marvelCharacterResponse);
        }

        private static bool ValidateRequest(Model.Request request, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(request.NameStartsWith))
            {
                error += "\nNameStartsWith is required. ";
            }

            return !string.IsNullOrEmpty(error);
        }

        private static async Task<MarvelCharacterResponse> GetMarvelCharacter(Request data, ILogger log)
        {
            string publicKey = Environment.GetEnvironmentVariable(MARVEL_PUBLIC_KEY);
            string hash = Environment.GetEnvironmentVariable(MARVEL_HASH);
            string timeStamp = Environment.GetEnvironmentVariable(MARVEL_TS);

            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(timeStamp))
            {
                throw new Exception("PublicKey, HashCode, TimeStamp is required. Check Enronments Variables ");
            }

            using (HttpClient client = new HttpClient())
            {
                // Construir URL de la petición
                string url = $"{MARVEL_URL}characters?apikey={publicKey}&ts={timeStamp}&hash={hash}";

                if (!string.IsNullOrEmpty(data.NameStartsWith))
                {
                    url += $"&nameStartsWith={Uri.EscapeDataString(data.NameStartsWith)}";
                }

                if (data.LimitRecords != 0)
                {
                    url += $"&limit={data.LimitRecords}";
                }

                log.LogInformation($"URL de solicitud: {url}");

                // Realizar solicitud HTTP
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode != true)
                {
                    throw new Exception("ERROR en httpRequest");
                }

                string content = response.Content.ReadAsStringAsync().Result;

                log.LogInformation("http Request OK");

                response.EnsureSuccessStatusCode();

                MarvelCharacterResponse apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<MarvelCharacterResponse>(content);

                return apiResponse;
            }
        }
    }
}
