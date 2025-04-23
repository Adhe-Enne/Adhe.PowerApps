using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;

namespace Biblioteca
{
    public class UniqueName : IPlugin
    {
        private IPluginExecutionContext _context;
        private Entity _entity;
        ITracingService _tracingService;
        IOrganizationService _service;

        public void Execute(IServiceProvider serviceProvider)
        {
            if (!LoadContext(serviceProvider))
                return;

            try
            {
                ValidateName();
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in Socio Plugin. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Socio Plugin" + ": {0}", ex.ToString());
                throw;
            }
        }

        private void ValidateName()
        {
            if (!_entity.Attributes.Any(x => x.Key == "dn_name"))
                return;

            string mainName = _entity.GetAttributeValue<string>("dn_name");

            if (string.IsNullOrEmpty(mainName))
            {
                _tracingService.Trace($"⛔ Nombre Invalido detectado: {mainName}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: Debe inrgesar un nombre Valido.");
            }

            QueryExpression query = new QueryExpression("dn_biblioteca")
            {
                ColumnSet = new ColumnSet("dn_name"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("dn_name", ConditionOperator.Equal, mainName);

            //si es update
            if (_entity.Id != Guid.Empty)
                query.Criteria.AddCondition("dn_bibliotecaid", ConditionOperator.NotEqual, _entity.Id);

            EntityCollection result = _service.RetrieveMultiple(query);

            if (result.Entities.Any())
            {
                _tracingService.Trace($"⛔ Nombre duplicado detectado: {mainName}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: El Nombre ya existe y no se puede duplicar.");
            }
        }

        private bool LoadContext(IServiceProvider serviceProvider)
        {
            _tracingService = (ITracingService) serviceProvider.GetService(typeof(ITracingService));
            _context = (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (!_context.InputParameters.Contains("Target") && !(_context.InputParameters["Target"] is Entity))
                return false;

            _entity = (Entity) _context.InputParameters["Target"];

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = serviceFactory.CreateOrganizationService(_context.UserId);

            return true;
        }
    }
}
