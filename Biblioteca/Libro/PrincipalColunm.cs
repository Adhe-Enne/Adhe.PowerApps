using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Libro
{
    public class PrincipalColunm : IPlugin
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
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in Libro Plugin." + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Libro Plugin" + ": {0}", ex.ToString());
                throw;
            }
        }

        private void SetPrincipalColunm()
        {
            string nombre = string.Empty;
            int isbn = 0;
            
            if (_context.MessageName == "Update")
            {
                var preImage = _context.PreEntityImages["updateLibro"];

                nombre = preImage.GetAttributeValue<string>("dn_nombre") ?? string.Empty;
                isbn = preImage.GetAttributeValue<int>("dn_isbn");
            }

            // Se contempla create al validar atributos
            if (_entity.Attributes.ContainsKey("dn_nombre"))
                nombre = _entity.GetAttributeValue<string>("dn_nombre") ?? string.Empty;

            if (_entity.Attributes.ContainsKey("dn_isbn"))
            {
                int isbnAux = _entity.GetAttributeValue<int>("dn_isbn");

                if (isbnAux <= 0)
                {
                    _tracingService.Trace($"⛔ ISBN Invalido detectado: {isbnAux}");
                    throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: El ISBN ingresado es invalido.");
                }

                isbn = isbnAux;
            }

            _entity["dn_name"] = $"{nombre} {isbn}";

            //log
            _tracingService.Trace("Nombre generado correctamente: " + _entity["dn_name"]);
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
