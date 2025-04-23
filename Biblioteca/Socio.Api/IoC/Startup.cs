using Microsoft.AspNetCore.Mvc;
using Socio.Api.Interfaces;
using Socio.Api.Services;

namespace Socio.Api.IoC
{
    public static class Startup
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddHttpClient();
            services.AddSingleton<IRequestService, RequestService>();
        }
    }
}
