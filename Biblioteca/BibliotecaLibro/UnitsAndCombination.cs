using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;

namespace BibliotecaLibro
{
    public class UnitsAndCombination : IPlugin
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
                SetPrincipalColunm();
                ValidateUnits();
                ValidateUniqueCombination();
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in BibliotecaLibro Plugin. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _tracingService.Trace("BibliotecaLibro Plugin" + ": {0}", ex.ToString());
                throw;
            }
        }

        private void ValidateUnits()
        {
            if (!_entity.Attributes.Any(x => x.Key == "dn_unidades"))
                return;

            int untis = _entity.GetAttributeValue<int>("dn_unidades");

            if (untis < 0 || untis > 10)
            {
                _tracingService.Trace($"⛔ Cantidade de unidades Invalido: {untis}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: Debe ingresar una cantidad entre 0 y 10.");
            }
        }

        private void ValidateUniqueCombination()
        {
            if (!_entity.Attributes.Any(x => x.Key == "dn_libro" || x.Key == "dn_biblioteca"))
                return;

            EntityReference bibliotecaRef = _entity.GetAttributeValue<EntityReference>("dn_biblioteca");
            EntityReference libroRef = _entity.GetAttributeValue<EntityReference>("dn_libro");

            if (bibliotecaRef == null || libroRef == null)
                return;

            QueryExpression query = new QueryExpression("dn_bibliotecalibro")
            {
                ColumnSet = new ColumnSet("dn_biblioteca", "dn_libro"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("dn_biblioteca", ConditionOperator.Equal, bibliotecaRef.Id);
            query.Criteria.AddCondition("dn_libro", ConditionOperator.Equal, libroRef.Id);

            // Si es una actualización, excluir el mismo ID
            if (_entity.Id != Guid.Empty)
                query.Criteria.AddCondition("dn_bibliotecalibroid", ConditionOperator.NotEqual, _entity.Id);

            EntityCollection result = _service.RetrieveMultiple(query);

            if (result.Entities.Any())
            {
                _tracingService.Trace($"⛔ La combinación Biblioteca {bibliotecaRef.Id} - Libro {libroRef.Id} ya existe.");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: Esta combinación de Biblioteca y Libro ya existe.");
            }
        }
        private void SetPrincipalColunm()
        {
            if (!_entity.Attributes.Any(x => x.Key == "dn_libro" || x.Key == "dn_biblioteca" || x.Key == "dn_unidades"))
                return;

            /*    if (bibliotecaRef == null || libroRef == null)
                    return;*/

            string biblioteca = string.Empty;
            string libro = string.Empty;

            if (_context.MessageName == "Update")
            {
                var preImage = _context.PreEntityImages["preImage"];

                biblioteca = GetEntityName("dn_biblioteca", preImage);
                libro = GetEntityName("dn_libro", preImage);
            }
            else if (_context.MessageName == "Create")
            {
                biblioteca = GetEntityName("dn_biblioteca", _entity);
                libro = GetEntityName("dn_libro", _entity);
                // Se contempla create al validar atributos
            }

            _entity["dn_name"] = $"{libro} - {biblioteca}";

            //log
            _tracingService.Trace("Nombre generado correctamente: " + _entity["dn_name"]);
        }

        private string GetEntityName(string entityLogicalName, Entity entity)
        {
            var entityRef = entity.GetAttributeValue<EntityReference>(entityLogicalName);
            Entity entityRefered = _service.Retrieve(entityLogicalName, entityRef.Id, new ColumnSet("dn_name"));
            return entityRefered?.GetAttributeValue<string>("dn_name") ?? string.Empty;
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
