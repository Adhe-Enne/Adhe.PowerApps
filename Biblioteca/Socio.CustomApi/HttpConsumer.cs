using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Socio.CustomApi
{
    public static class HttpConsumer
    {
        public static async Task<string> GetSocioExternalEnviroment(string dniSocio, string url, string code)
        {
            string responseJson = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                //(mg_Socio/mg_dni eq 39741404) and (mg_Socio/mg_socioid ne null)

                string requestUrl = $"{url}/api/Socio/Crm?code={code}";
                requestUrl = $"{requestUrl}&filter=(mg_dni eq {dniSocio})&EntityName=mg_socio&useExternalUrl=true&mapperName=MG_SOC_GET_CRM";
                HttpResponseMessage response = client.GetAsync(requestUrl).Result;

                //if (!response.IsSuccessStatusCode)
                //    throw new Exception($"Error en la solicitud: {response.StatusCode} - {response.RequestMessage.ToString()}");

                responseJson = response.Content.ReadAsStringAsync().Result;
            }

            return responseJson;
        }

        public static async Task<string> GetAlquilerExternalEnviroment(string socioGuid, string url, string code)
        {
            string responseJson = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{url}/api/Alquiler/Crm?code={code}";
                requestUrl = $"{requestUrl}&filter=(_mg_socio_value eq {socioGuid})&EntityName=mg_alquiler&useExternalUrl=true&mapperName=MG_ALQ_GET_ENT";
                HttpResponseMessage response = client.GetAsync(requestUrl).Result;

                responseJson = await response.Content.ReadAsStringAsync();
            }

            return responseJson;
        }

        public static async Task<string> SaveSocioExternalEnviroment(string parametersBody, string url, string code)
        {
            string responseJson = string.Empty;

            try
            {
                HttpClient client = new HttpClient();
                string requestUrl = $"{url}/api/Socio/Crm?code={code}";
                var values = ToBodyData("MG_SOC_POST_CRM", "mg_socio", parametersBody);
                var jsonContent = new StringContent(values, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(requestUrl, jsonContent).Result;
                /*
                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en la solicitud: {response.StatusCode} - {responseBody} - {response.RequestMessage.ToString()}");
                }*/

                responseJson = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                responseJson  = ex.Message;
            }
            
            return responseJson;
        }

        public static async Task<string> SaveAlquilerExternalEnviroment(string parametersBody, string url, string code)
        {
            string responseJson = string.Empty;

            try
            {
                HttpClient client = new HttpClient();

                string requestUrl = $"{url}/api/Alquiler/Crm?code={code}";
                var values = ToBodyData("MG_ALQ_POST_CRM", "mg_alquiler", parametersBody);
                var jsonContent = new StringContent(values, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(requestUrl, jsonContent).Result;

                responseJson = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                responseJson = ex.Message;
            }

            return responseJson;
        }
        public static string ToBodyData(string mapperName, string entityName, string body)
        {
            var bodyObject = Newtonsoft.Json.JsonConvert.DeserializeObject(body);
            var values = new Dictionary<string, object>
            {
                { "UseExternalUrl", "true" },
                { "MapperName", mapperName },
                { "EntityName", entityName },
                { "Body", bodyObject }
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(values);
        }

        public static async Task<string> GetBibliotecaExternalEnviroment(string url, string code)
        {
            string responseJson = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{url}/api/Socio/BibliotecaLibro?code={code}";
                requestUrl = $"{requestUrl}&filter=(mg_unidades gt 0)&EntityName=mg_bibliotecalibro&useExternalUrl=true&mapperName=MG_BIBLIB_GET_ENT";
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                //if (!response.IsSuccessStatusCode)
                //    throw new Exception($"Error en la solicitud: {response.StatusCode} - {response.RequestMessage.ToString()}");

                responseJson = await response.Content.ReadAsStringAsync();
            }

            return responseJson;
        }
        //_mg_socio_value
    }
}
