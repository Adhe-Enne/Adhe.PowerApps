using CrmEarlyBound;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace Socio
{
    public class DniAndIdentityNroSoc : IPlugin
    {
        private IPluginExecutionContext _context;
        private Entity _entity;
        ITracingService _tracingService;
        IOrganizationService _service;
        private dn_Socio _socio;

        public void Execute(IServiceProvider serviceProvider)
        {
            if(!LoadContext(serviceProvider))
                return;

            try
            {
                SetPrincipalColunm();
                ValidateDNI();
                AssignNroSocio();

                _tracingService.Trace("Se utilizo entidades por EarlyBound");
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

        private void SetPrincipalColunm()
        {
            string nombre=string.Empty;
            string apellido = string.Empty;

            if (_context.MessageName== "Update")
            {
                var preImage = _context.PreEntityImages["updateSocio"].ToEntity<dn_Socio>();

                nombre = preImage.dn_Nombre;
                apellido = preImage.dn_Apellido;
            }

            // Se contempla create al validar atributos
            if (_socio.Attributes.ContainsKey("dn_nombre"))
                nombre = _socio.dn_Nombre;

            if (_socio.Attributes.ContainsKey("dn_apellido"))
                apellido = _socio.dn_Apellido;

            _socio.dn_Name = $"{nombre} {apellido}";
            _entity = _socio;
            //log
            _tracingService.Trace("Nombre generado correctamente: " + _entity["dn_name"]);
        }

        private void ValidateDNI()
        {
            if (!_socio.Attributes.Any(x => x.Key == "dn_dni"))
                return;

            //int dni = _entity.GetAttributeValue<int>("dn_dni");

            if (_socio.dn_DNI <= 0)
            {
                _tracingService.Trace($"⛔ Valor DNI invalido");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: El DNI ingresado es invalido.");
            }

            QueryExpression query = new QueryExpression("dn_socio")
            {
                ColumnSet = new ColumnSet("dn_dni"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("dn_dni", ConditionOperator.Equal, _socio.dn_DNI);

            //si es update
            if (_entity.Id != Guid.Empty)
                query.Criteria.AddCondition("dn_socioid", ConditionOperator.NotEqual, _entity.Id);

            EntityCollection result = _service.RetrieveMultiple(query);

            if (result.Entities.Any())
            {
                _tracingService.Trace($"⛔ DNI duplicado detectado: {_socio.dn_DNI}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: El DNI ya existe y no se puede duplicar.");
            }
        }

        private void AssignNroSocioWithExpression()
        {
            if (_context.MessageName != "Create")
                return;

            QueryExpression query = new QueryExpression("dn_socio")
            {
                ColumnSet = new ColumnSet("dn_nrodesocio"),
                Orders = { new OrderExpression("dn_nrodesocio", OrderType.Descending) },
                TopCount = 1
            };

            EntityCollection result = _service.RetrieveMultiple(query);

            int lastSeed = 0;

            if (result.Entities.Any())
                lastSeed = result.Entities.First().GetAttributeValue<int>("dn_nrodesocio");

            int newSeed = lastSeed + 1;
            _entity["dn_nrodesocio"] = newSeed;
            _tracingService.Trace($"📌 Nuevo Número de Socio asignado: {newSeed}");
        }

        private void AssignNroSocio()
        {
            //solo contemplo create
            if (_context.MessageName != "Create")
                return;

            //filtramos en forma descendente y tomamos el primero
            string fetchXml = @"
                <fetch top='1'>
                    <entity name='dn_socio'>
                        <attribute name='dn_nrodesocio' />
                        <order attribute='dn_nrodesocio' descending='true' />
                    </entity>
                </fetch>";

            EntityCollection result = _service.RetrieveMultiple(new FetchExpression(fetchXml));

            int lastSeed = 0;

            if (result.Entities.Any())
                lastSeed = result.Entities.First().GetAttributeValue<int>("dn_nrodesocio");

            int newSeed = lastSeed + 1;
            _entity["dn_nrodesocio"] = newSeed;
            _tracingService.Trace($"📌 Nuevo Número de Socio asignado con FetchXML: {newSeed}");
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

            _socio = _entity.ToEntity<dn_Socio>();

            return true;
        }
    }
}