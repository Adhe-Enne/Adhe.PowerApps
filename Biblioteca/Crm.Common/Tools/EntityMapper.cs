using Core.External;
using Core.Framework.Exceptions;
using Core.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
namespace Crm.Common.Tools
{
    public class EntityMapper//<T> where T : class, new()
    {
        private readonly Dictionary<string, string> _mapping;
        private readonly Dictionary<string, MappingRule> _mappingRule;
        private readonly Entity _entity;

        private EntityMapper(string jsonMapping)
        {
            _mapping = Json.ToDictionary(jsonMapping);

            if (!_mapping.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }
        }

        private EntityMapper(string jsonMapping, string entityLogicalName)
        {
            _mappingRule = Json.ToDictionary<string, MappingRule>(jsonMapping);

            if (!_mappingRule.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }

            _entity = new Entity(entityLogicalName);
        }

        public static Entity Map<T>(T obj, string entityLogicalName, Dictionary<string, MappingRule> mappingRule) where T : class
        {
            if (!mappingRule.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }

            var objType = typeof(T);
            Entity entity = new Entity(entityLogicalName);

            foreach (var entry in mappingRule)
            {
                var rule = entry.Value;
                var prop = objType.GetProperty(rule.Source);

                var value = prop.GetValue(obj);

                if (value == null)
                {
                    entity[entry.Key] = ConvertToDataType(entry.Value);
                    continue;
                }

                entity[entry.Key] = ConvertToDataType(entry.Value, value);
            }

            return entity;
        }

        public static object ConvertToDataType(MappingRule rule, object value = null)
        {
            object result = null;

            if (value is null) value = rule.Default;

            if (rule.Type == "EntityReference" && value is string strValue)
            {
                result = new EntityReference(rule.Entity, Guid.Parse(strValue));
            }
            else if (rule.Type == "DateTime" && value is DateTime dateValue)
            {
                result = dateValue.ToUniversalTime();
            }
            else if (rule.Type == "DateTime" && rule.Default == "now+4d")
            {
                result = value ?? DateTime.UtcNow.AddDays(4);
            }
            else if ((rule.Type.ToLower() == "option" || rule.Type.ToLower() == "choice") && int.TryParse(value.ToString(), out int parsedInt) )
            {
                result = new OptionSetValue(parsedInt);
            }
            else if (rule.Type.ToLower() == "int" && int.TryParse(value.ToString(), out parsedInt))
            {
                result = parsedInt;
            }
            else
            {
                result = value;
            }

                return result;
        }

        public static List<T> Map<T>(List<Entity> entities, Dictionary<string, string> mapping) where T : class, new()
        {
            if (!mapping.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }

            List<T> result = new List<T>();
            foreach (var entity in entities)
            {
                result.Add(Map<T>(entity, mapping));
            }

            return result;
        }

        public static T Map<T>(Entity entity, Dictionary<string, string> mapping) where T : class, new()
        {
            if (!mapping.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }

            T obj = new T();
            Type objType = typeof(T);

            foreach (var prop in objType.GetProperties())
            {
                if (mapping.TryGetValue(prop.Name, out string entityField) && entity.Contains(entityField))
                {
                    object value = entity[entityField];

                    // Si es un EntityReference, asignamos el GUID y el Name en los campos correspondientes
                    if (value is EntityReference er)
                    {
                        if (prop.PropertyType == typeof(Guid))
                        {
                            value = er.Id; // Asignar GUID
                        }
                        else if (prop.PropertyType == typeof(string))
                        {
                            value = er.Name; // Asignar el Name del EntityReference
                        }
                    }

                    if (prop.PropertyType == typeof(DateTime) && value is DateTime dt)
                    {
                        value = dt.ToLocalTime();  // Convertir fechas a LocalTime
                    }

                    prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                }
            }

            return obj;
        }

        public static List<T> Map<T>(string json, Dictionary<string, string> mapping) where T : class, new()
        {
            if (!mapping.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }

            var jsonArray = JArray.Parse(json);
            var resultList = new List<T>();

            foreach (var jObject in jsonArray)
            {
                var obj = new T();
                foreach (var entry in mapping)
                {
                    var property = typeof(T).GetProperty(entry.Key, BindingFlags.Public | BindingFlags.Instance);
                    if (property == null) continue;

                    var value = GetNestedValue(jObject, entry.Value);
                    if (value == null) continue;

                    if (property.PropertyType == typeof(Guid))
                    {
                        property.SetValue(obj, Guid.Parse(value.ToString()));
                    }
                    else if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(obj, DateTime.Parse(value.ToString()));
                    }
                    else
                    {
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    }
                }
                resultList.Add(obj);
            }

            return resultList;
        }

        public static Dictionary<string, string> ToDiccionary<T>(T obj, Dictionary<string, string> mapping)
        {
            if (!mapping.Any())
            {
                throw new ResultException("No se ha definido un mapeo para la entidad.", System.Net.HttpStatusCode.InternalServerError);
            }

            var finalParameters = new Dictionary<string, string>();
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            foreach (var param in mapping)
            {
                string value = param.Value;

                var prop = properties.FirstOrDefault(prop => value.Contains($"{{{prop.Name}}}"));

                if (prop is not null)
                {
                    object propValue = prop.GetValue(obj);

                    var placeHolder = $"{{{prop.Name}}}";

                    if (propValue is DateTime dateTimeValue)
                    {
                        value = value.Replace(placeHolder, dateTimeValue.ToString("o"));
                    }
                    else
                    {
                        value = value.Replace(placeHolder, propValue?.ToString() ?? string.Empty);
                    }

                    //value = value.Replace($"{{{prop.Name}}}", propValue?.ToString() ?? string.Empty);
                }

                finalParameters[param.Key] = value;
            }

            return finalParameters;
        }

        private static object GetNestedValue(JToken jObject, string path)
        {
            var tokens = path.Split('.');
            JToken token = jObject;

            foreach (var t in tokens)
            {
                if (token == null) return null;
                token = token[t];
            }

            return token?.ToObject<object>();
        }
    }
}
