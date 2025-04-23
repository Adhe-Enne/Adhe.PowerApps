using Core.Framework;

namespace Core.Abstractions
{
    public interface IApiResultData<T>: IApiResult where T : class
    {
        public DataResult<T> Data { get; set; }
    }

    public interface IApiResultCrmData<T> : IApiResult where T : class
    {
        public DataBasicResult<T> Data { get; set; }
    }

    public interface IDataResult<T> where T : class
    {
        public int Total { get; set; }
        public List<T> Result { get; set; }
    }
}
