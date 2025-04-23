using Core.Model;
using Crm.Common.Dto;
using Crm.Common.Model;
using Microsoft.Xrm.Sdk;

namespace Crm.Common.Interfaces
{
    public interface ICrmService
    {
        public Task RestartCrmClient();
        public Task ChangeDestiny(bool UseExternalDataverse = false);

        public Task<string> GetHttpToken();

        public Task<string> GetCrmToken();
        /// <summary>
        /// Obtiene las credenciales necesarias para el acceso al CRM.
        /// Este método asincrónico permite obtener las credenciales necesarias para la autenticación y acceso al sistema CRM.
        /// Las credenciales pueden ser obtenidas desde el entorno de ejecución o desde una URL externa de Dataverse, 
        /// dependiendo de los parámetros proporcionados.
        /// </summary>
        /// <param name="GetFromEnviroment">Indica si se deben obtener las credenciales desde el entorno. Este parámetro es opcional y de configuración.</param>
        /// <param name="UseExternalUrlDataverse">Indica si se debe usar una URL externa para Dataverse. Este parámetro es opcional y de configuración.</param>
        /// <returns>Una tarea que representa la operación asincrónica. El resultado contiene las credenciales necesarias para el acceso al CRM.</returns>
        public Credential GetCredential(bool UseExternalDataverse = false);

        public Task<EntityCollection> GetRecords(IRequestGet query);
        public Task<string> GetRecordsHttpClient(IRequestGet query);
        public Task<List<T>> GetRecordsCrm<T>(IRequestGet query) where T : class, new();
        public Task<Guid> PostHttpClient<T>(IRequestData<T> reqContent) where T : class, new();
        public Task<Guid> PostCrm<T>(IRequestData<T> reqContent) where T : class, new();
    }
}
