using Alquiler.CustomApi.Fields;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace Alquiler.CustomApi
{
    public class Devolver : IPlugin
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
                DevolverCustomAction();
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _context.OutputParameters[Dn_Alquiler.MessageOut] = ex.Message;
                throw new InvalidPluginExecutionException($"An error occurred in CustomApi Plugin: {ex.Message} \n {ex.Detail.TraceText}", ex);
            }
            catch (Exception ex)
            {
                _context.OutputParameters[Dn_Alquiler.MessageOut] = ex.Message;
                _tracingService.Trace("CustomApi Plugin" + ": {0}", ex.ToString());
                throw;
            }
        }

        public void DevolverCustomAction()
        {
            _context.OutputParameters[Dn_Alquiler.DevolucionOkOut] = false;

            if (!_context.InputParameters.Contains(Dn_Alquiler.AlquilerIn) || !(_context.InputParameters[Dn_Alquiler.AlquilerIn] is EntityReference alquilerRef))
                throw new InvalidPluginExecutionException("⛔ Error: No se proporcionó una referencia válida de alquiler.");

            _tracingService.Trace($"🔄 Procesando devolución para Alquiler ID: {alquilerRef.Id}");

            // Obtener datos del alquiler
            _entity = _service.Retrieve(Dn_Alquiler.Entity, alquilerRef.Id, new ColumnSet(Dn_Alquiler.StateCode, Dn_Alquiler.StatusReason));

            if (_entity == null)
                throw new InvalidPluginExecutionException("⛔ Error: El alquiler no existe.");

            OptionSetValue estadoActualOption = _entity.GetAttributeValue<OptionSetValue>(Dn_Alquiler.StateCode);
            int estadoActual = estadoActualOption != null ? estadoActualOption.Value : 0; //

            if (estadoActual == 1) // 2 = Finalizado
            {
                _tracingService.Trace("⚠️ El alquiler ya está finalizado.");
                throw new InvalidPluginExecutionException("⚠️ El alquiler ya está finalizado.");
            }

            _entity[Dn_Alquiler.StateCode] = new OptionSetValue(1);
            _entity[Dn_Alquiler.StatusReason] = new OptionSetValue(2);
            //_entity[Dn_Alquiler.FechaDevolucion] = DateTime.Now;
            _service.Update(_entity);

            _tracingService.Trace("✅ Devolución registrada con éxito.");

            _context.OutputParameters[Dn_Alquiler.MessageOut] = "✅ Devolución registrada con éxito.";
            _context.OutputParameters[Dn_Alquiler.DevolucionOkOut] = true;
        }

        private bool LoadContext(IServiceProvider serviceProvider)
        {
            _tracingService = (ITracingService) serviceProvider.GetService(typeof(ITracingService));
            _context = (IPluginExecutionContext) serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = serviceFactory.CreateOrganizationService(_context.UserId);

            return true;
        }
    }
}
