namespace MyV2ray.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RayPortContainer
    {
        public IList<RayPort> Inbounds { get; set; }
    }

    public class RayConfig
    {
        public const string DefaultConfigFilePathOnLinux = @"/etc/v2ray/config.json";

        public const string DefaultConfigFilePathOnWindows = @"C:/Users/laggage/Desktop/config.json";

        public static string ConfigFilePath { get; private set; }

        static RayConfig()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                ConfigFilePath = DefaultConfigFilePathOnLinux;
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                ConfigFilePath = DefaultConfigFilePathOnWindows;
        }

        #region Constructor

        public RayConfig(string configFilePath = "")
        {
            if(!string.IsNullOrEmpty(configFilePath))
                ConfigFilePath = configFilePath;
        }

        #endregion

        public RayPortContainer RayPorts =>
            JsonConvert.DeserializeObject<RayPortContainer>(
                File.ReadAllText(ConfigFilePath, Encoding.Default), RayConfigJsonSetting.JsonSerializerSettings);

        public static JObject RayConfigJObject => JsonConvert.DeserializeObject<JObject>(
            File.ReadAllText(ConfigFilePath), RayConfigJsonSetting.JsonSerializerSettings);
    }
}
