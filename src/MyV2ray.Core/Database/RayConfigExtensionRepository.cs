namespace MyV2ray.Core.Database
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MyV2ray.Core.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;

    public class RayConfigExtensionRepository
    {
        private object syncRoot = new object();

        private JObject rootJObj;
        private JObject RootJObj
        {
            get
            {
                if (rootJObj == null)
                {
                    lock(syncRoot)
                        if (rootJObj == null)
                        {
                            if (!File.Exists(RayConfigExtension.RayConfigExtensionFilePath))
                            {
                                FileStream fs = File.Create(RayConfigExtension.RayConfigExtensionFilePath);
                                fs.Close();
                                fs.Dispose();
                            }
                           
                            string jsonText = File.ReadAllText(RayConfigExtension.RayConfigExtensionFilePath);
                            jsonText = string.IsNullOrEmpty(jsonText) ? "{}" : jsonText;
                            rootJObj = JsonConvert.DeserializeObject<JObject>(jsonText);
                        }
                }
                return rootJObj;
            }
        }

        public string GetHostAddress()
        {
            string address = null;
            try
            {
                address = RootJObj.SelectToken("hostAddress").ToObject<string>();
            }
            catch
            {
                address = "127.0.0.1";
            }
            return address;
        }

        public void SetHostAddress(string hostAddress)
        {
            const string propertyName = "hostAddress";
            if (!RootJObj.ContainsKey(propertyName))
            {
                RootJObj.Add(propertyName, hostAddress);
            }
            else
                RootJObj.Property(propertyName).Value = hostAddress;

            SaveToFile();
        }

        private void SaveToFile()
        {
            File.WriteAllText(
                RayConfigExtension.RayConfigExtensionFilePath,RootJObj?.ToString());
        }

        public static RayConfigExtensionRepository Create() => new RayConfigExtensionRepository();

        public void AddForbiddenUser(RayPort port, RayPortUser user)
        {
            JObject rootJObj = this.rootJObj;
            RayConfigExtension configEx =
                JsonConvert.DeserializeObject<RayConfigExtension>(
                    rootJObj.ToString());

            if (configEx.ForbiddenedUsersPorts == null)
                configEx.ForbiddenedUsersPorts = new List<RayPort>();
            else if (!configEx.ForbiddenedUsersPorts.Contains(
                port,
                RayPortEqualityComparer.Default))
                configEx.ForbiddenedUsersPorts.Add(port);

            RayPort p = configEx.ForbiddenedUsersPorts.FirstOrDefault(
                r => r.Port == port.Port);

            ((p.Settings??=new RayPortSettings())
                .Clients??=new List<RayPortUser>()).Add(user);

            SaveConfigEx(configEx);
        }

        public void AddForbiddenUser(int portNumber, RayPortUser user)
        {
            AddForbiddenUser(new RayPort {Port = portNumber}, user);
        }

        private void SaveConfigEx(RayConfigExtension configEx)
        {
            File.WriteAllText(
                RayConfigExtension.RayConfigExtensionFilePath,
                JsonConvert.SerializeObject(
                    configEx, 
                    RayConfigJsonSetting.JsonSerializerSettings));
        }
    }
}
