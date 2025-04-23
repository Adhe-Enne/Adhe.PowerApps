using Socio.Api.Result;

namespace Socio.Api.Interfaces
{
    public interface IApiResultData<T>: IApiResult
    {
        public DataResult<T> Data { get; set; }
    }

    public interface IDataResult<T> : Interfaces.IParametersQuery  // Cuando es tipo List<T>
    {
        public int Total { get; set; }
        public T Result { get; set; }
    }
}
