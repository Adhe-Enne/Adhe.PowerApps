using Core.Abstractions;
using Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class DataResult<T> : StandarFilter, IDataResult<T> where T : class
    {
        public DataResult(List<T> data, IRequestFilter query) : base(query)
        {
            Result = data;
            Total = data.Count;
        }

        public DataResult(List<T> data)
        {
            Result = data;
            Total = data.Count;
        }

        public int Total { get; set; }

        [JsonPropertyOrder(10)] // 🔹 Se mostrará primero
        public List<T> Result { get; set; }
    }

    public class DataCrmResult<T> : BasicFilter, IDataResult<T> where T : class
    {
        public DataCrmResult(List<T> data, IRequestFilter query) : base(query)
        {
            Result = data;
            Total = data.Count;
        }

        public DataCrmResult(List<T> data)
        {
            Result = data;
            Total = data.Count;
        }

        public int Total { get; set; }
        [JsonPropertyOrder(10)] // 🔹 Se mostrará primero
        public List<T> Result { get; set; }
    }

    public class DataBasicResult<T> : IDataResult<T> where T : class
    {
        public DataBasicResult(List<T> data)
        {
            Result = data;
            Total = data.Count;
        }

        public int Total { get; set; }
        [JsonPropertyOrder(10)] // 🔹 Se mostrará primero
        public List<T> Result { get; set; }
    }
}
