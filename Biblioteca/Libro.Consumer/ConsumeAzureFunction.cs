using Microsoft.Xrm.Sdk;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Contexts;
namespace Libro.Consumer
{
    public class ConsumeAzureFunction : IPlugin
    {
        ITracingService _tracingService;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));
            _tracingService = (ITracingService) serviceProvider.GetService(typeof(ITracingService));

            try
            {
                if (!context.InputParameters.Contains("NombreIn") || !(context.InputParameters["NombreIn"] is string nombre))
                    return;

                if (string.IsNullOrEmpty(nombre))
                    throw new InvalidPluginExecutionException("Debe proporcionar un nombre de libro");

                _tracingService.Trace($"Libro a enviar: {nombre}");

                ApiResponse response = CallAzureFunction(nombre).GetAwaiter().GetResult();

                string json = JsonConvert.SerializeObject(response);
                context.OutputParameters["ConsumerSuccess"] = response.Success;
                context.OutputParameters["ConsumerMessage"] = response.Message;
                context.OutputParameters["ConsumerContent"] = response.Content;
                _tracingService.Trace("Consumo API Azure exitoso");
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Error en el plugin: " + ex.Message);

                context.OutputParameters["ConsumerSuccess"] = false;
                context.OutputParameters["ConsumerMessage"] = "Error en la llamada a la API";
                context.OutputParameters["ConsumerContent"] = ex.Message;
                throw new InvalidPluginExecutionException("Error en el plugin: " + ex.Message);
            }
        }

        private async Task<ApiResponse> CallAzureFunction(string nombre)
        {
            string url = "https://libro-dn.azurewebsites.net/api/Libro";
            string token = "HjEmz5xUkaIbO0iSWYKyS7cOp1o0raIBcqI_0HtEQPesAzFubtBsRg==";
            string securityName = "code";

            bool usarPost = true;
            HttpClient client = new HttpClient();
            HttpResponseMessage response;
            
            if (usarPost)
            {
                var jsonBody = SerializeOneValue(nombre);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{url}?{securityName}={token}")
                {
                    Content = content
                };
                response = await client.SendAsync(request);
            }
            else
            {
                var requestUrl = $"{url}?{securityName}={token}&nombre={Uri.EscapeDataString(nombre)}";
                response = await client.GetAsync(requestUrl);
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                _tracingService.Trace("Error en la llamada a la API " + errorMessage);

                return new ApiResponse
                {
                    Success = false,
                    Message = "Error en la llamada a la API",
                    Content = errorMessage
                };
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(responseContent) ?? new ApiResponse
            {
                Success = false,
                Message = "No se pudo obtener respuesta de la API",
                Content = string.Empty
            };
        }

        private string SerializeOneValue(string nombre)
        {
            return JsonConvert.SerializeObject(new { nombre });
        }


        // Clase para modelar la respuesta de la API
        [DataContract]

        private class ApiResponse
        {
            [DataMember]
            public bool Success { get; set; }
            [DataMember]
            public string Message { get; set; }
            [DataMember]
            public string Content { get; set; }
        }
    }
}
