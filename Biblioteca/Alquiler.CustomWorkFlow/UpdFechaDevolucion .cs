using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace Alquiler.CustomWorkFlow
{
    public class UpdFechaDevolucion : CodeActivity
    {
        [Input("Alquiler ID")]
        [RequiredArgument]
        [ReferenceTarget("dn_alquiler")]

        public InArgument<EntityReference> Alquiler { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService TracingService = executionContext.GetExtension<ITracingService>();

            TracingService.Trace($"UpdFechaDevolucion: Inicio - Try #{context.Depth}");

            if (context.Depth > 2)
            {
                TracingService.Trace($"UpdFechaDevolucion: Se evita loop de ejecucion: ");
                return; // 🚫 Ya se ejecutó, evitamos el bucle
            }

            Guid alquilerGuid = this.Alquiler.Get(executionContext).Id;

            TracingService.Trace($"Alquiler GUID: {alquilerGuid}");

            Entity alquiler = service.Retrieve("dn_alquiler", alquilerGuid, new ColumnSet("statuscode", "dn_fechadevolucion"));

            if (alquiler == null)
            {
                TracingService.Trace("Alquiler no encontrado");
                return;
            }

            int estado = ((OptionSetValue) alquiler["statuscode"]).Value;
            TracingService.Trace($"Estado actual: {estado}");

            if (estado == 2) // Reemplazar con el código correcto de "Finalizado"
            {
                alquiler["dn_fechadevolucion"] = DateTime.UtcNow;
                service.Update(alquiler);
                TracingService.Trace("Fecha de devolución actualizada correctamente");
            }
            else
                TracingService.Trace("El estado no es Finalizado, no se actualiza la fecha");
        }
    }
}
