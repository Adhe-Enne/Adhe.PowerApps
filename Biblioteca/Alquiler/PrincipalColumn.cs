using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace Alquiler
{
    public class PrincipalColumn : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            if (bag.PluginContext.MessageName != "Create" &&  bag.PluginContext.MessageName != "Update") return;

            EntityReference socioRef = null;
            EntityReference libroRef = null;

            if (bag.PluginContext.MessageName == "Update")
            {
                if (IsInvalidPreImage(bag, out socioRef, out libroRef)) return; //por el momento sirve para ignorar procesos de plugin en cadena
            }

            if (bag.TargetEntity.Attributes.ContainsKey("dn_libro"))
                libroRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_libro");

            if (bag.TargetEntity.Attributes.ContainsKey("dn_socio"))
                socioRef = bag.TargetEntity.GetAttributeValue<EntityReference>("dn_socio");

            if (socioRef == null || libroRef == null)
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "Las referencias devolvieron un valor nulo.");

            Entity libroEntity = bag.Service.Retrieve("dn_libro", libroRef.Id, new ColumnSet("dn_nombre"));
            Entity socioEntity = bag.Service.Retrieve("dn_socio", socioRef.Id, new ColumnSet("dn_name"));

            if (libroEntity == null || socioEntity == null)
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "Las referencias no devolvieron una relacion valida");

            string libro = libroEntity.GetAttributeValue<string>("dn_nombre");
            string socio = socioEntity.GetAttributeValue<string>("dn_name");

            if (string.IsNullOrEmpty(libro) || string.IsNullOrEmpty(socio))
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "El nombre del libr/socio no puede ser vacio o nulo, verifique reg en tablas.");

            bag.TargetEntity["dn_name"] = $"{libro} - {socio}";

            //log
            bag.TracingService.Trace("Nombre generado correctamente: " + bag.TargetEntity["dn_name"]);
        }

        private bool IsInvalidPreImage(PluginBag bag, out EntityReference socioRef, out EntityReference libroRef)
        {
            socioRef = null;
            libroRef = null;   

            if (!bag.PluginContext.PreEntityImages.Contains("preImage")) return true;

            Entity preImage = bag.PluginContext.PreEntityImages["preImage"];

            if (!preImage.Attributes.Contains("dn_socio") || !preImage.Attributes.Contains("dn_libro")) return true;

            socioRef = preImage.GetAttributeValue<EntityReference>("dn_socio");
            libroRef = preImage.GetAttributeValue<EntityReference>("dn_libro");

            return false;
        }
    }
}
