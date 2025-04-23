using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace Alquiler
{
    public class UniqueRent : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            if (!bag.TargetEntity.Attributes.Contains("dn_socio") || !bag.TargetEntity.Attributes.Contains("dn_libro"))
                return;

            EntityReference socioRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_socio");
            EntityReference libroRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_libro");

            if (socioRef == null || libroRef == null)
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "Se detecto referencias invalidas o nulas.");

            QueryExpression query = new QueryExpression("dn_alquiler")
            {
                ColumnSet = new ColumnSet("dn_alquilerid"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("dn_socio", ConditionOperator.Equal, socioRef.Id);
            query.Criteria.AddCondition("dn_libro", ConditionOperator.Equal, libroRef.Id);

            EntityCollection result = bag.Service.RetrieveMultiple(query);

            if (result.Entities.Count > 0)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "Este socio ya tiene alquilado este libro.");
            }
        }
    }
}
