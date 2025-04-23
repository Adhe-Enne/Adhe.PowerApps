using Crm.Access.Exceptions;
using Socio.Api.Model;
using System.Net;
using System.Text.RegularExpressions;

namespace Socio.Api.Tools
{
    public static class OdataHelper
    {
        private static readonly Dictionary<string, string> OperatorMap = new()
        {
            { " eq ", "eq" }, { " ne ", "neq" },
            { " gt ", "gt" }, { " lt ", "lt" },
            { " ge ", "ge" }, { " le ", "le" }
        };

        private static readonly Dictionary<string, string> FunctionMap = new()
        {
            { "contains", "like" },
            { "startswith", "like" },
            { "endswith", "like" }
        };

        public static string BuildFetchXml(ParametersQuery query, string entityName)
        {
            string top = query.Top.HasValue ? $"top='{query.Top}' " : "";
            string page = query.Page.HasValue ? $"page='{query.Page}' " : "";
            string count = query.Rows.HasValue ? $"count='{query.Rows}' " : "";
            string orderBy = string.Empty;

            query.OrderBy?.ForEach(x =>
                {
                    orderBy += $"<order attribute='{x}' />";
                }
            );

            string fetchXml = $@"
                <fetch {top} {page} {count}>
                    <entity name='{entityName}'>{(!string.IsNullOrEmpty(query.Filter) ? $"<filter type='and'>{OdataHelper.ConvertODataToFetchXml(query.Filter)}</filter>" : "")}
                        {orderBy}
                    </entity>
                </fetch>";

            return fetchXml;
        }
        
        public static string BuildFilterUrl(ParametersQuery query)
        {
            string filter = string.Empty;



            if (!string.IsNullOrEmpty(query.Filter))
            {
                query.Filter = query.Filter.Trim();
                query.Filter = query.Filter.StartsWith('(') ? query.Filter : "("+query.Filter;   
                query.Filter = query.Filter.EndsWith(')') ? query.Filter : query.Filter + ")";   

                filter += $"${query.Filter}";
            }

            filter += query.Top.HasValue ? $"&$top={query.Top}" : "";
            //filter += query.Page.HasValue ? $"&$page='{query.Page}' " : "";
            //filter += query.Rows.HasValue ? $"&$count='{query.Rows}' " : "";
 
            if (query.OrderBy is not null && query.OrderBy.Any())
            {
                filter += "&$orderby=";
                query.OrderBy.ForEach(x =>
                {
                    filter += $"{x},";
                });
                filter = filter.TrimEnd(',');
            }

          if (filter.StartsWith("&"))
            {
                filter = filter.Remove(0,1);
            }

            return !string.IsNullOrEmpty(filter) ? $"?{filter}": "";
        }

        public static string ConvertODataToFetchXml(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return "";

            var conditions = new List<string>();
            var filters = filter.Split(new[] { " and ", " or " }, StringSplitOptions.None);

            foreach (var condition in filters)
            {
                string attribute, fetchOperator, fetchValue;

                if (TryExtractFunctionCondition(condition, out attribute, out fetchOperator, out fetchValue) ||
                    TryExtractComparisonCondition(condition, out attribute, out fetchOperator, out fetchValue))
                {
                    conditions.Add($"<condition attribute='{attribute}' operator='{fetchOperator}' value='{fetchValue}' />");
                }
                else
                {
                    throw new ResultException("Formato 'Filter' INVALIDO.", HttpStatusCode.BadRequest);
                }
            }

            return string.Join("", conditions);
        }

        private static bool TryExtractFunctionCondition(string condition, out string attribute, out string fetchOperator, out string fetchValue)
        {
            foreach (var function in FunctionMap.Keys)
            {
                var match = Regex.Match(condition, $@"{function}\(([^,]+),\s*'([^']+)'\)");
                if (match.Success)
                {
                    attribute = match.Groups[1].Value.Trim();
                    fetchValue = function switch
                    {
                        "contains" => $"%{match.Groups[2].Value.Trim()}%",
                        "startswith" => $"{match.Groups[2].Value.Trim()}%",
                        "endswith" => $"%{match.Groups[2].Value.Trim()}",
                        _ => throw new InvalidOperationException("Función no soportada")
                    };
                    fetchOperator = FunctionMap[function];
                    return true;
                }
            }

            attribute = fetchOperator = fetchValue = null;
            return false;
        }

        private static bool TryExtractComparisonCondition(string condition, out string attribute, out string fetchOperator, out string fetchValue)
        {
            condition = condition.Trim('(', ')');
            condition = condition.Trim('_').Replace("_value", "");

            foreach (var op in OperatorMap.Keys)
            {
                var parts = condition.Split(new[] { op }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    attribute = parts[0].Trim();
                    fetchValue = parts[1].Trim().Trim('\''); // Elimina comillas si es string
                    fetchOperator = OperatorMap[op];
                    return true;
                }
            }

            attribute = fetchOperator = fetchValue = null;
            return false;
        }
    }
}
