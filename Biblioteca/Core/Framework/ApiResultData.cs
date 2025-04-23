using Core.Abstractions;
using System.Text.Json.Serialization;

namespace Core.Framework
{
    /// <summary>
    /// Aplica solo a los resultados que devuelven datos paginados
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResultData<T> : ApiResult, IApiResultData<T> where T : class
    {
        [JsonPropertyOrder(6)] // 🔹 Se mostrará primero

        public DataResult<T> Data { get; set; }

        public ApiResultData()
        {
            Data = null;
        }

        public void AddData(List<T> data)
        {
            Data = new DataResult<T>(data);
        }

        public void AddData(List<T> data, IRequestFilter query)
        {
            Data = new DataResult<T>(data, query);
        }
    }

    public class ApiResultCrmData<T> : ApiResult, IApiResultCrmData<T> where T : class
    {
        [JsonPropertyOrder(6)] // 🔹 Se mostrará primero

        public DataBasicResult<T> Data { get; set; }

        public ApiResultCrmData()
        {
            Data = null;
        }

        public void AddData(List<T> data)
        {
            Data = new DataBasicResult<T>(data);
        }
    }
}
