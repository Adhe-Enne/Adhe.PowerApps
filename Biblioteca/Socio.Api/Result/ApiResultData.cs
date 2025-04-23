using Socio.Api.Interfaces;
using Socio.Api.Model;
using System.Text.Json.Serialization;

namespace Socio.Api.Result
{
    /// <summary>
    /// Aplica solo a los resultados que devuelven datos paginados
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResultData<T> : ApiResult, IApiResultData<T>
    {
        [JsonPropertyOrder(6)] // 🔹 Se mostrará primero

        public DataResult<T> Data { get; set; }

        public ApiResultData()
        {
            Data = null;
        }

        public ApiResultData<T> SetErrorResult(string Message)
        {
            Set(Message, true);

            return this;
        }

        public void AddData(List<T> data)
        {
            this.Data = new DataResult<T>(data);
        }

        public void AddData(List<T> data, ParametersQuery query)
        {
            this.Data = new DataResult<T>(data, query);
        }
    }

    public class DataResult<T> : ParametersQuery
    {
        public DataResult(List<T> data, ParametersQuery query)
        {
            this.Top = query.Top;
            this.Page = query.Page;
            this.Rows = query.Rows;
            this.Result = data;
            this.Total = data.Count;
        }

        public DataResult(List<T> data)
        {
            this.Result = data;
            this.Total = data.Count;
        }

        public int Total { get; set; }

        [JsonPropertyOrder(10)] // 🔹 Se mostrará primero
        public List<T> Result { get; set; }
    }
}
