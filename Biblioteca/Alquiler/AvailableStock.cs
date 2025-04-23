using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using System.Workflow.Runtime.Tracking;
using System;

namespace Alquiler
{
    public class AvailableStock : PluginBase
    {
        public override void Execute(PluginBag bag)
        {

            if (!bag.TargetEntity.Attributes.Any(x => x.Key == "dn_biblioteca"  || x.Key == "dn_libro"))
                return;

            EntityReference bibliotecaRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_biblioteca");
            EntityReference libroRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_libro");

            if (bibliotecaRef == null || libroRef == null)
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "Se detecto referencias invalidas o nulas.");

            // FetchXML con Aggregate
            string fetchXml = $@"
            <fetch aggregate='true'>
                <entity name='dn_bibliotecalibro'>
                    <attribute name='dn_unidades' alias='stock_total' aggregate='sum' />
                    <filter type='and'>
                        <condition attribute='dn_biblioteca' operator='eq' value='{bibliotecaRef.Id}' />
                        <condition attribute='dn_libro' operator='eq' value='{libroRef.Id}' />
                    </filter>
                </entity>
            </fetch>";

            EntityCollection result = bag.Service.RetrieveMultiple(new FetchExpression(fetchXml));

            int stockDisponible = 0;

            if (result.Entities.Count > 0)
            {
                var aggregateStock = result.Entities[0].GetAttributeValue<AliasedValue>("stock_total");
                if (aggregateStock != null)
                    stockDisponible = (int)aggregateStock.Value;
            }

            bag.Trace($"📌 Stock disponible para Biblioteca: {bibliotecaRef.Id}, Libro: {libroRef.Id} → {stockDisponible}");

            if (stockDisponible == 0)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "⚠️ Error: No hay stock disponible para alquilar este libro en esta biblioteca.");
            }

            //extra: actualizar stock
            //UpdateStock(bag, bibliotecaRef.Id, libroRef.Id);
        }

        private void UpdateStock(PluginBag bag, Guid idBiblioteca, Guid idLibro)
        {
            QueryExpression query = new QueryExpression("dn_bibliotecalibro")
            {
                ColumnSet = new ColumnSet("dn_unidades")
            };
            query.Criteria.AddCondition("dn_biblioteca", ConditionOperator.Equal, idBiblioteca);
            query.Criteria.AddCondition("dn_libro", ConditionOperator.Equal, idLibro);

            EntityCollection result = bag.Service.RetrieveMultiple(query);

            if (result.Entities.Count > 0)
            {
                Entity bibliotecaLibro = result.Entities.First();
                int stockActual = bibliotecaLibro.GetAttributeValue<int>("dn_unidades");

                // Verificamos que haya stock disponible
                if (stockActual > 0)
                {
                    bibliotecaLibro["dn_unidades"] = stockActual - 1; // Restamos una unidad
                    bag.Service.Update(bibliotecaLibro);
                    bag.TracingService.Trace($"✅ Stock actualizado: {stockActual - 1}");
                }
                else
                    throw new InvalidPluginExecutionException("❌ No hay stock disponible para alquilar este libro.");
            }
            else
            {
                throw new InvalidPluginExecutionException("❌ No se encontró el stock del libro en la biblioteca.");
            }
        }
    }
}
