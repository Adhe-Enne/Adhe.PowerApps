using Axxon.PluginCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socio.CustomApi
{
    public class CreateSocioExternalEnviroment : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            string responseJson = string.Empty;
            string url = bag.GetEnvironmentVariableValue(Constants.FUNCTION_URL);
            string code = bag.GetEnvironmentVariableValue(Constants.FUNCTION_CODE);

            if (!bag.PluginContext.InputParameters.Contains(Constants.SocioBodyIn) || !(bag.PluginContext.InputParameters[Constants.SocioBodyIn] is string dniSocio))
                throw new Exception("El Json string de Socio es obligatorio");

            bag.PluginContext.OutputParameters[Constants.ResponseMessage] = HttpConsumer.SaveSocioExternalEnviroment(dniSocio, url, code).Result;
        }
    }
}
