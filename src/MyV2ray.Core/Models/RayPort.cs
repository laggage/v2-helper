namespace MyV2ray.Core.Models
{
    using System;

    public class RayPort
    {
        public string Listen { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public string Tag { get; set; }

        public RayPortSettings Settings { get; set; }

        public RayPortStreamSettings StreamSettings { get; set; }

        public RayPort()
        {
            Protocol = "vmess";
            Port = (new Random()).Next(1, 65535);
            Listen = "0.0.0.0";
            Tag = null;
            Settings = new RayPortSettings();
            StreamSettings = new RayPortStreamSettings();
        }
    }
}
