using Axxon.PluginCommons;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Socio.CustomApi
{
    public class CheckSocioByDni : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            string responseJson = string.Empty;
            string url = bag.GetEnvironmentVariableValue(Constants.FUNCTION_URL);
            string code = bag.GetEnvironmentVariableValue(Constants.FUNCTION_CODE);

            if (!bag.PluginContext.InputParameters.Contains(Constants.DniSocioIn) || !(bag.PluginContext.InputParameters[Constants.DniSocioIn] is string dniSocio))
                throw new Exception("Parametro Dni de Socio es mandatorio");

            bag.PluginContext.OutputParameters[Constants.SocioOut] = HttpConsumer.GetSocioExternalEnviroment(dniSocio, url, code).Result;
        }
    }
}
