using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.External
{
    public static class JsonExtensions
    {
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null) ||
                   (token.Type == JTokenType.Undefined);
        }
    }

    public static class Json
    {
        public static string Serialize(object obj, Formatting formatting = Formatting.Indented)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, formatting);
        }

        public static T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static Dictionary<string, string> ToDictionary(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static Dictionary<K,V> ToDictionary<K, V>(string json) where K : notnull
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<K, V>>(json);
        }      
    }
}
