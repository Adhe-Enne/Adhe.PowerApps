using Axxon.PluginCommons;
using System;

namespace Socio.CustomApi
{
    public class CreateAlquilerExternalEnvironment : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            //string nameStartsWith = bag.PluginContext.InputParameters["DniSocioIn"].ToString();
            string responseJson = string.Empty;
            string url = bag.GetEnvironmentVariableValue(Constants.FUNCTION_URL);
            string code = bag.GetEnvironmentVariableValue(Constants.FUNCTION_CODE);

            if (!bag.PluginContext.InputParameters.Contains(Constants.AlquilerIn) || !(bag.PluginContext.InputParameters[Constants.AlquilerIn] is string dniSocio))
                throw new Exception("Parametro Alquiler de Socio es mandatorio");

            responseJson = HttpConsumer.SaveAlquilerExternalEnviroment(dniSocio, url, code).Result;
            bag.PluginContext.OutputParameters[Constants.ResponseMessage] = responseJson;
        }
    }
}
