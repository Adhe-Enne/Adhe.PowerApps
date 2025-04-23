using Axxon.PluginCommons;
using CrmEarlyBound;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace Alquiler
{
    public class RentPartnerPending : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            if (!bag.TargetEntity.Attributes.Any(x => x.Key == "dn_socio"))
                return;

            bag.Trace("Consultando Alquiler mediante LinQ");

            if (CheckPendingRentByLinQ(bag) >= 2)
            {
                bag.Trace($"📌 El socio ingresado ya tiene 2 alquileres pendientes!");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: El socio ingresado ya tiene 2 alquileres pendientes.");
            }
        }

        private int CheckPendingRentByLinQ(PluginBag bag)
        {
            EntityReference socioRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_socio");

            var context = new OrganizationServiceContext(bag.Service);
            return context.CreateQuery<dn_Alquiler>()
               .Where(x => x.dn_Socio.Id == socioRef.Id
                           && (x.dn_Estado == dn_EstadoAlquiler.Pendiente // || x.dn_FechaDevolucion != null // por el momento validamos solo estado
                            && x.StateCode == dn_AlquilerState.Active && x.StatusCode == dn_Alquiler_StatusCode.Entregado
                           )).ToList().Count();
        }

        private int CheckPendingRentByFetch(PluginBag bag)
        {
            EntityReference socioRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_socio");

            string fetchXml = $@"
            <fetch aggregate='true'>
                <entity name='dn_alquiler'>
                    <attribute name='dn_alquilerid' alias='pendingRent' aggregate='count' />
                    <filter type='and'>
                        <condition attribute='dn_socio' operator='eq' value='{socioRef.Id}' />
                        <condition attribute='dn_estado' operator='eq' value='1' />
                        <condition attribute='dn_fechadevolucion' operator='null' />
                    </filter>
                </entity>
            </fetch>";

            EntityCollection result = bag.Service.RetrieveMultiple(new FetchExpression(fetchXml));

            return result.Entities.Count;
        }
    }
}
