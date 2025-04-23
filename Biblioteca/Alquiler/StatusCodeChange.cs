using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alquiler
{
    public class StatusCodeChange : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            if (!bag.TargetEntity.Attributes.ContainsKey("dn_fechadevolucion"))
                return;

            DateTime? fechaDevolucion = bag.TargetEntity.GetAttributeValue<DateTime>("dn_fechadevolucion");

            if ((fechaDevolucion != null) && fechaDevolucion >= DateTime.MinValue)
            {
                bag.TargetEntity["dn_estado"] = new OptionSetValue(2);
                bag.Trace("Alquiler devuelto y Finalizado!");
            }
        }
    }
}
