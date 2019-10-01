namespace MyV2ray.Core.Models
{
    using System;
    using Newtonsoft.Json;

    public class RayPortUser
    {
        public const string EmailSuffix = "@la.laggage";

        [JsonProperty("id")]
        public string Uuid { get; set; }
        public int AlterId { get; set; }
        public int Level { get; set; }
        public string Email { get; set; }

        public RayPortUser()
        {
            Uuid = Guid.NewGuid().ToString();
            Level = 1;
            AlterId = 64;
        }

        public override string ToString()
        {
            return string.Format("{1}, {0}", Uuid, Email);
        }
    }
}
