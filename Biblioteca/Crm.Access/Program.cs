using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Crm.Access.IoC;
using Core.Model;
using Crm.Common.Model;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
// Register ITokenRequestService and its implementation TokenRequestService as a service
builder.Services.AddServices();
builder.Services.AddHttpClient();
builder.Build().Run();
