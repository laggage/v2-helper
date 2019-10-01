namespace MyV2ray.Core.Extensions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class JsonExtensions
    {
        public static JObject ToJObject(this string text, JsonLoadSettings settings)
        {
            return JObject.Parse(text, settings);
        }

        public static JObject ToJObject<T>(this T o, JsonSerializer settings)
        {
            return JObject.FromObject(o, settings);
        }

        public static string ToJsonString<T>(this T o, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(o, settings);
        }
    }
}
