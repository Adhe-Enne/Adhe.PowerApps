using Core.Model;
using Microsoft.Xrm.Sdk;

namespace Crm.Common.Interfaces
{
    public interface IClientService
    {
        public Task<EntityCollection> GetRecords(StandarFilter query, bool fromEnv = false);
        public Task<string> GetRecordsHttpClient(StandarFilter query, bool useEnvCfg = false);
    }
}
