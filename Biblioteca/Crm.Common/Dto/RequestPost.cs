using Core.Abstractions;
using Core.Model;
using Crm.Common.Interfaces;

namespace Crm.Common.Dto
{
    public class RequestPost<T> : RequestMetadata, IRequestData<T>
    {
        public T Body { get; set; }
    }


    public class RequestMetadata : IRequestData
    {
        public bool UseExternalUrl { get; set; }
        public string MapperName { get; set; }
        public string EntityName { get; set; }
    }

    public class RequestGet : StandarFilter, IRequestGet
    {
        public bool UseExternalUrl { get; set; }
        public string MapperName { get; set; }
        public string EntityName { get; set; }
    }

    public class RequestGeneric : RequestGet;
}
