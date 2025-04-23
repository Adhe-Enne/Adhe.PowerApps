using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Socio.CustomApi
{
    public class GetLocalSocioByReference : PluginBase
    {
        public override void Execute(PluginBag bag)
        {

            if (!bag.PluginContext.InputParameters.Contains(Constants.SocioReferenceIn) || !(bag.PluginContext.InputParameters[Constants.SocioReferenceIn] is EntityReference entity))
                throw new Exception("Parametro Alquiler de Socio es mandatorio");

            bag.PluginContext.OutputParameters[Constants.SocioOut] = bag.Service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
        }
    }
}
