using Crm.Common.Constants;
using Crm.Common.Dto;
using Crm.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Crm.Access.FunctionTools
{
    public static class FunctionHelper
    {
        public static IRequestGet ToParameters(IQueryCollection query)
        {
            RequestGeneric parameters = new();

            if (query.ContainsKey(QueryKeys.Page))
                parameters.Page = int.Parse(query[QueryKeys.Page]);

            if (query.ContainsKey(QueryKeys.Rows))
                parameters.Rows = int.Parse(query[QueryKeys.Rows]);

            if (query.ContainsKey(QueryKeys.Filter))
                parameters.Filter = query[QueryKeys.Filter];

            if (query.ContainsKey(QueryKeys.Top))
                parameters.Top = int.Parse(query[QueryKeys.Top]);

            if (query.ContainsKey(QueryKeys.EntityName))
                parameters.EntityName = query[QueryKeys.EntityName];

            if (query.ContainsKey(QueryKeys.Select))
                parameters.Select = query[QueryKeys.Select];

            if (query.ContainsKey(QueryKeys.Expand))
                parameters.Expand = query[QueryKeys.Expand]; 

            if (query.ContainsKey(QueryKeys.Mapper))
                parameters.MapperName = query[QueryKeys.Mapper];

            if (query.ContainsKey(QueryKeys.UseExternalUrl))
            {
                bool.TryParse(query[QueryKeys.UseExternalUrl], out bool useExternalUrl);
                parameters.UseExternalUrl = useExternalUrl;
            }

            return parameters;
        }

        public static string GetJsonMapper(string mapperName)
        {
            var mappers = Core.External.Json.ToDictionary(Environment.GetEnvironmentVariable(CrmKey.ENTITY_MAPPERS));
            var mapKey = mappers.GetValueOrDefault(mapperName) ?? string.Empty;
            var json = Environment.GetEnvironmentVariable(mapKey) ?? string.Empty;
            return json;
        }

        public static string GetJsonMapperEnv(string mapperName)
        {
            var json = Environment.GetEnvironmentVariable(mapperName) ?? string.Empty;
            return json;
        }
    }
}
