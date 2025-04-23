using Axxon.PluginCommons;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Socio.CustomApi
{
    public class GetAlquilerExternalEnviroment : PluginBase
    {

        public override void Execute(PluginBag bag)
        {
            //string nameStartsWith = bag.PluginContext.InputParameters["DniSocioIn"].ToString();
            string responseJson = string.Empty;
            string url = bag.GetEnvironmentVariableValue(Constants.FUNCTION_URL);
            string code = bag.GetEnvironmentVariableValue(Constants.FUNCTION_CODE);

            if (!bag.PluginContext.InputParameters.Contains(Constants.GuidSocioIn) || !(bag.PluginContext.InputParameters[Constants.GuidSocioIn] is string socioId))
                throw new Exception("Parametro Dni de Socio es mandatorio");

            responseJson = HttpConsumer.GetAlquilerExternalEnviroment(socioId, url, code).Result;
            bag.PluginContext.OutputParameters[Constants.AlquileresOut] = responseJson;
        }
    }
}
