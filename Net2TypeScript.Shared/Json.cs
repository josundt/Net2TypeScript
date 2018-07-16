using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace jasMIN.Net2TypeScript.Shared
{
    public static class Json
    {
        public static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            //ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, Settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
