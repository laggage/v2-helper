namespace MyV2ray.Console.Repository
{
    using System.Collections.Generic;
    using MyV2ray.Core.Models;

    class RayConfigExtension
    {
        public string HostAddress { get; set; }
        public IList<RayPort> ForbiddenUser { get; set; }
    }
}
