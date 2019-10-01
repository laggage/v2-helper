namespace MyV2ray.Core.Models
{
    using MyV2ray.Core.Database;
    using MyV2ray.Core.Extensions;
    using Newtonsoft.Json;

    public class ClientUserConfig
    {
        /// <summary>
        /// 服务器ip或域名
        /// </summary>
        [JsonProperty("add")]
        public string Address { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty("v")]
        public string Version { get; set; } 

        /// <summary>
        /// 备注别名
        /// </summary>
        [JsonProperty("ps")]
        public string Alias { get; set; }

        /// <summary>
        /// 服务器端口号
        /// </summary>
        public int Port { get; set; }

        [JsonProperty("id")]
        public string Uuid { get; set; }

        /// <summary>
        /// 额外id
        /// </summary>
        [JsonProperty("aid")]
        public int AlterId { get; set; }

        /// <summary>
        /// 传输协议（tcp\kcp\ws\h2\quic)
        /// </summary>
        [JsonProperty("net")]
        public string NetWork { get; set; }

        /// <summary>
        /// 伪装类型
        /// </summary>
        [JsonProperty("type")]
        public string PretendType { get; set; }

        /// <summary>
        /// 伪装的域名
        /// </summary>
        public string Host { get; set; }

        public string Path { get; set; }

        /// <summary>
        /// 底层传输安全（tls)
        /// </summary>
        public string Tls { get; set; }

        public ClientUserConfig()
        {
        }

        public ClientUserConfig(RayPort rayPort, RayPortUser rayUser)
        {
            Address = RayConfigExtensionRepository.Create().GetHostAddress();
            Version = "2";
            Alias = rayUser.GetRayPortUserRemark();
            Port = rayPort.Port;
            Uuid = rayUser.Uuid;
            AlterId = rayUser.AlterId;
            NetWork = rayPort.StreamSettings?.NetWork;
            PretendType = "none";
            Host = ""; // 伪装域名
            Tls = rayPort.StreamSettings?.TlsSettings == null ? "": "tls";
            Path = rayPort.StreamSettings?.WSSettings?.Path ?? string.Empty;
        }
    }
}
