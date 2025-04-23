using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;

namespace Libro
{
    public class UniqueISBN : IPlugin
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
                ValidateISBN();
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in Libro Plugin. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Libro Plugin" + ": {0}", ex.ToString());
                throw;
            }
        }

        private void ValidateISBN()
        {
            if (!_entity.Attributes.Any(x => x.Key == "dn_isbn"))
                return;

            int isbn = _entity.GetAttributeValue<int>("dn_isbn");

            if (isbn <= 0)
            {
                _tracingService.Trace($"⛔ ISBN Invalido detectado: {isbn}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: Debe ingresar un ISBN valido.");
            }

            _tracingService.Trace("Consultando Socio mediante LinQ v3");

            if (QueryISBNWithLinQ(isbn))
            {
                _tracingService.Trace($"⛔ ISBN duplicado detectado: {isbn}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: El ISBN ya existe y no se puede duplicar.");
            }
        }

        private bool QueryISBNWithLinQ(int isbn)
        {
            var context = new OrganizationServiceContext(_service);
            return context.CreateQuery("dn_libro").Where(l =>
                    l.GetAttributeValue<int>("dn_isbn") == isbn
                    && l.GetAttributeValue<Guid>("dn_libroid") != _entity.Id)
                .ToList().Any();
        }

        private bool QueryISBNWithFetch(int isbn)
        {
            QueryExpression query = new QueryExpression("dn_libro")
            {
                ColumnSet = new ColumnSet("dn_isbn"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("dn_isbn", ConditionOperator.Equal, isbn);

            //si es update
            if (_entity.Id != Guid.Empty)
                query.Criteria.AddCondition("dn_libroid", ConditionOperator.NotEqual, _entity.Id);

            EntityCollection result = _service.RetrieveMultiple(query);

            return result.Entities.Any();
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
