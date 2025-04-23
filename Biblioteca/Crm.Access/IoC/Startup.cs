using Crm.Common.Interfaces;
using Crm.Common.Model;
using Crm.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Access.IoC
{
    public static class Startup
    {
        public static void AddServices(this IServiceCollection services)
        {
            //  services.AddScoped(typeof(IAuthService), typeof(AuthService)); // Asegúrate de que esta interfaz tenga una implementación
            services.AddSingleton(typeof(ICrmService), typeof(CrmService)); // Asegúrate de que esta interfaz tenga una implementación
            var sett = CrmSettings.LoadFromEnvironment();

            sett.IS_FUNCTION_APP = true;
            services.AddSingleton(sett);
        }
    }
}
