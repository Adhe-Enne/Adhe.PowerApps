using Axxon.PluginCommons;

namespace Socio.CustomApi
{
    public class GetBibliotecas : PluginBase
    {
        public override void Execute(PluginBag bag)
        {
            string responseJson = string.Empty;
            string url = bag.GetEnvironmentVariableValue(Constants.FUNCTION_URL);
            string code = bag.GetEnvironmentVariableValue(Constants.FUNCTION_CODE);

            bag.PluginContext.OutputParameters[Constants.BibliotecasOut] = HttpConsumer.GetBibliotecaExternalEnviroment(url, code).Result;
        }
    }
}
