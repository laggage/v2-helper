namespace MyV2ray.Core.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// V2Ray扩展配置
    /// </summary>
    class RayConfigExtension
    {
        public const string RayConfigExtensionFilePath = "./configExtension.json";

        /// <summary>
        /// 主机域名或ip
        /// </summary>
        public string HostAddress { get; set; }

        public IList<RayPort> ForbiddenedUsersPorts { get; set; }
    }
}
