namespace MyV2ray.Core.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class RayConfigJsonSetting
    {
        public static JsonSerializer JsonSerializer => new JsonSerializer
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            }
        };

        public static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
        {
            
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            }
        };
    }
}
