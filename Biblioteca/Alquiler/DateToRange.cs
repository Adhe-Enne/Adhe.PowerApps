using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Alquiler
{
    public class DateToRange : PluginBase
    {
        public override void Execute(PluginBag bag)
        {

            if (!bag.TargetEntity.Attributes.Any(x => x.Key == "dn_desde" || x.Key == "dn_biblioteca" || x.Key == "dn_hasta"))
                return;

            DateTime desde = bag.TargetEntity.GetAttributeValue<DateTime>("dn_desde");
            DateTime hasta = bag.TargetEntity.GetAttributeValue<DateTime>("dn_hasta");

            if (desde > hasta)
            {
                bag.Trace($"Fecha 'Hasta' es menor que  'Desde'. Desde {desde.ToString()} - Hasta {hasta.ToString()}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "❌ La fecha 'Hasta' no puede ser menor que la fecha 'Desde'.");
            }

            // Calcular la diferencia en días
            TimeSpan diferencia = hasta - desde;

            if (diferencia.TotalDays > 7)
            {
                bag.Trace($"Fecha 'Hasta' es menor que  'Desde'. Desde {desde.ToString()} - Hasta {hasta.ToString()}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "❌ La fecha 'Hasta' no puede ser mayor a 7 días desde la fecha 'Desde'.");
            }

            bag.Trace("Validacion de Hora exitosa");
        }
    }
}
