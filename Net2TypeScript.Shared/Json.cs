using System.Text.Json;

namespace jasMIN.Net2TypeScript.Shared
{
    public static class Json
    {
        private static JsonSerializerOptions Settings => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string Serialize<T>(T o)
        {
            return JsonSerializer.Serialize<T>(o, Settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Settings);
        }
    }
}
