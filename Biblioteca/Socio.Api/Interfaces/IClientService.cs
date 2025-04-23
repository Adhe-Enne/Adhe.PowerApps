using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using Socio.Api.Model;

namespace Socio.Api.Interfaces
{
    public interface IClientService
    {
        public EntityCollection GetRecords(ParametersQuery query);
        public Task<string> GetRecordsHttpClient(ParametersQuery query);
        public void UpdateCustomer(Guid socioId);
        public void DisableRentals(Guid socioId);
    }
}
