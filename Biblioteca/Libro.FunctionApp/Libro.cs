using Libro.FunctionApp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Libro.FunctionApp
{
    public static class LibroFunction
    {
        [FunctionName("Libro")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Procesando solicitud en la función Azure 'Libro'.");

            string libro = null;

            if (req.Method == HttpMethods.Get)
            {
                libro = req.Query["nombre"];
            }
            else if (req.Method == HttpMethods.Post)
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                RequestBody data = JsonConvert.DeserializeObject<RequestBody>(requestBody);
                libro = data.Nombre;
            }

            if (string.IsNullOrEmpty(libro))
            {
                log.LogWarning("Debe proporcionar un nombre de libro en la querystring o en el cuerpo de la solicitud");
                return new ConflictObjectResult(new
                {
                    Success = false,
                    Message = "Debe proporcionar un nombre de libro en la querystring o en el cuerpo de la solicitud.",
                    Content = string.Empty
                });
            }

            log.LogInformation("Solicitud procesada correctamente: Hola {libro}");

            return new OkObjectResult(new
            {
                Success = true,
                Message = "Solicitud procesada correctamente.",
                Content = $"Hola {libro}"
            });
        }
    }
}