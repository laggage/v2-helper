namespace MyV2ray.Core.Database
{
    using MyV2ray.Core.Models;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;
    using MyV2ray.Core.Extensions;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Text;

    public class RayConfigRepository
    {
        private readonly RayConfig rayConfig;

        public RayConfigRepository(string configFilePath = "")
        {
            rayConfig = new RayConfig(configFilePath);
        }

        public IList<RayPort> GetRayPorts() => rayConfig.RayPorts.Inbounds;

        public IList<RayPortUser> GetRayPortUsers(int rayPortNumber)
        {
            var rayPorts = GetRayPorts();
            if (!(rayPorts.FirstOrDefault(r => r.Port == rayPortNumber) is RayPort rayPort))
                throw new ArgumentOutOfRangeException(nameof(rayPortNumber), "端口号不存在.");
            return rayPort.Settings.Clients;
        }

        public IDictionary<RayPort, IList<RayPortUser>> GetRayPortsUsersDict()
        {
            var rayPorts = GetRayPorts();
            IDictionary<RayPort, IList<RayPortUser>> map = new Dictionary<RayPort, IList<RayPortUser>>();
            foreach (RayPort rayPort in rayPorts)
                map.Add(rayPort, rayPort.Settings?.Clients);
            return map;
        }

        public IList<RayPortUser> GetRayPortsUsers()
        {
            var repo = new RayConfigRepository();

            return repo.GetRayPorts().SelectMany(r => r.Settings?.Clients ?? new List<RayPortUser>()).ToList();
        }

        public void AddUserToPort(RayPort rayPort, RayPortUser user)
        {
            var rayConfigObj = GetRayConfigJObject();

            if (!(rayConfigObj.SelectToken($"inbounds[?(@port == {rayPort.Port})].settings.clients") is JArray rayPortUsersObj))
                throw new ArgumentException(nameof(rayPort), "端口不存在, 请创建端口");

            JObject userObj = JObject.FromObject(user, RayConfigJsonSetting.JsonSerializer);
            rayPortUsersObj.Add(userObj);

            WriteJsonToFile(rayConfigObj);
        }

        public void AddPort(RayPort port)
        {
            var rayPorts = GetRayPorts();

            if (rayPorts.Any(p => p.Port == port.Port))
                throw new ArgumentOutOfRangeException(nameof(port), $"端口号{port.Port}冲突");


            if (rayPorts.Any(p => p.Settings?.Clients?.Any(c =>
            {
                bool confict = false;
                foreach (var client in port.Settings?.Clients ?? new List<RayPortUser>())
                    if (client.Uuid == c.Uuid)
                    {
                        confict = true;
                        break;
                    }
                return confict;
            }) ?? false))
                throw new ArgumentOutOfRangeException(nameof(port), $"用户id冲突");

            JObject rayConfigObj = GetRayConfigJObject();

            if (rayConfigObj.SelectToken("inbounds") is JArray rayPortsObj)
            {
                rayPortsObj.Add(ToJObject(port));
                WriteJsonToFile(rayConfigObj);
            }
        }

        private static void WriteJsonToFile(JObject jObj)
        {
            File.WriteAllText(RayConfig.ConfigFilePath, jObj.ToString());
        }

        private JObject ToJObject<T>(T obj)
        {
            return JObject.FromObject(obj, RayConfigJsonSetting.JsonSerializer);
        }

        private JObject GetRayConfigJObject()
        {
            return RayConfig.RayConfigJObject as JObject;
        }

        public void UpdatePort(int portToModify, RayPort toModify)
        {
            JObject rayConfig = GetRayConfigJObject();
            JArray portsObj = rayConfig.SelectToken("inbounds") as JArray;

            int index = -1;
            if (portsObj.FirstOrDefault(
                j => j["port"].ToString() == portToModify.ToString()) is JObject portObj)
            {
                index = portsObj.IndexOf(portObj);
                portsObj.Remove(portObj);
            }

            portsObj.Insert(
                index, 
                JObject.FromObject(toModify, RayConfigJsonSetting.JsonSerializer));
            
            WriteJsonToFile(rayConfig);
        }

        public RayPort GetRayPort(int portNumber)
        {
            return GetRayPorts().FirstOrDefault(r => r.Port == portNumber);
        }

        public static void UpdateUser(string uuid, RayPortUser rayPortUser)
        {
            RayConfigRepository repo = new RayConfigRepository();
            var rayPort = repo.GetRayPorts()
                .FirstOrDefault(
                    o => o.Settings
                        .Clients
                        ?.Contains(rayPortUser, RayPortUserEqualityComparer.Default) ?? false);
            var rayPortUsers = rayPort.Settings.Clients;
            int index = rayPortUsers.IndexOf(
                rayPortUsers.FirstOrDefault(r => r.Uuid == uuid));
            int port = rayPort.Port;

            JObject rayConfigObj = repo.GetRayConfigJObject();

            JArray clientsObj = rayConfigObj.SelectToken($"inbounds[?(@port == {port})].settings.clients") as JArray;

            if (!(rayConfigObj
                .SelectToken($"inbounds[?(@port == {port})].settings.clients[?(@id == '{uuid}')]") is JObject clientObjToUpdate)) return;

            clientsObj.Remove(clientObjToUpdate);
            clientsObj.Insert(index, JObject.FromObject(rayPortUser, RayConfigJsonSetting.JsonSerializer));

            WriteJsonToFile(rayConfigObj);
        }

        public static RayPort GetRayPort(RayPortUser rayPortUser)
        {
            var repo = new RayConfigRepository();
            var rayPorts = repo.GetRayPorts();
            RayPort rayPort = null;
            foreach (var r in rayPorts)
            {
                if (r.Settings?.Clients?.Any(obj => obj.Uuid == rayPortUser.Uuid) ?? false)
                {
                    rayPort = r;
                    break;
                }
            }
            return rayPort;
        }

        public void DeleteRayPort(RayPort rayPort)
        {
            var configObj = GetRayConfigJObject();

            if (!(configObj.SelectToken("inbounds") is JArray rayPortsObj)) throw new Exception("列表中没有可用的端口.");

            var rayPortObj = configObj.SelectToken($"inbounds[?(@port == {rayPort.Port})]");

            if (rayPortObj == null) throw new Exception($"找不到端口{rayPort.Port}");

            rayPortsObj.Remove(rayPortObj);

            WriteJsonToFile(configObj);
        }

        public void DeleteRayPortUser(RayPortUser rayPortUser)
        {
            var rayPort = GetRayPort(rayPortUser);
            var rayConfigObj = GetRayConfigJObject();

            if (!(rayConfigObj.SelectToken(
                $"inbounds[?(@port == {rayPort.Port})].settings.clients") is JArray rayUsersObj))
                throw new ArgumentException($"找不到用户{rayPortUser.GetRayPortUserRemark()}");

            var rayUserObjtoDelete = rayUsersObj.SelectToken($"$.[?(@.id == '{rayPortUser.Uuid}')]");
            if (rayUserObjtoDelete == null)
                throw new ArgumentException($"找不到用户{rayPortUser.GetRayPortUserRemark()}");

            if (!rayUsersObj.Remove(rayUserObjtoDelete))
                throw new Exception($"Failed to remove {rayUserObjtoDelete.ToString()}");

            WriteJsonToFile(rayConfigObj);
        }

        public void UpdateUser(RayPortUser configToModify)
        {
            if (configToModify == null)
                throw new ArgumentException(nameof(configToModify));

            JObject configJObj = GetRayConfigJObject();

            if (configJObj == null) throw new Exception("无法读取配置文件.");

            int portNum = GetRayPort(configToModify).Port;

            JArray usersJObj =
                configJObj
                    .SelectToken(
                        $"inbounds[?(@port == {portNum})].settings.clients") as JArray;
            
            if (!(usersJObj
                .SelectToken($"$.[?(@.id == '{configToModify.Uuid}')]") 
                is JObject userToModifyJObj))
                throw new Exception($"找不到指定的用户!{configToModify}");

            int pos = usersJObj.IndexOf(userToModifyJObj);

            usersJObj.Remove(userToModifyJObj);
            usersJObj.Insert(pos, configToModify.ToJObject(RayConfigJsonSetting.JsonSerializer));
            WriteJsonToFile(configJObj);
        }

        /// <summary>
        /// 运行v2ctl命令
        /// </summary>
        /// <param name="v2ctlPath">v2ctl的路径</param>
        /// <param name="args">启动参数</param>
        /// <returns>v2ctl的输出内容</returns>
        private string RunV2Ctl(string v2ctlPath , string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo(
                v2ctlPath,
                args)
            {
                RedirectStandardOutput = true,
                StandardInputEncoding = Encoding.Default,
                RedirectStandardInput = true
            };
            
            Process proc = Process.Start(psi);
            if (proc == null)
                throw new InvalidOperationException("Failed to start v2ctl to query traffic usage.");

            string output = null;
            using (var sr = proc.StandardOutput)
                output = sr?.ReadToEnd();
            return output;
        }

        /// <summary>
        /// 配置流量统计功能
        /// </summary>
        /*
         * "stats": {},
            "api": {
                "tag": "api",
                "services": [
                    "StatsService"
                ]
            },
            "policy": {
                "levels": {
                    "0": {
                        "statsUserUplink": true,
                        "statsUserDownlink": true
                    }
                },
                "system": {
                    "statsInboundUplink": true,
                    "statsInboundDownlink": true
                }
            },
         */
        public static void ConfigTrafficStatistic()
        {
            JObject rootJObj = RayConfig.RayConfigJObject;
            bool hasChange = false;

            if (!rootJObj.ContainsKey("stats"))
            {
                rootJObj.Add("stats", JObject.Parse("{}"));
                hasChange = true;
            }
            if (!rootJObj.ContainsKey("api") || 
                (!(rootJObj.SelectToken("api.services") as JArray)?.Children().Contains("StatsService") ?? false))
            {
                var api = new
                {
                    tag = "api",
                    services = new string[] {"StatsService", "LoggerService", "HandlerService" },
                };
                rootJObj.Add("api", JObject.FromObject(api)); 
                hasChange = true;
            }

            if (!rootJObj.ContainsKey("policy"))
            {
                var policy = new
                {
                    levels = new Dictionary<string, object>
                    {
                        {
                            "1", new Dictionary<string, bool>
                            {
                                {"statsUserUplink", true},
                                {"statsUserDownlink", true},
                            }
                        },
                    },
                    system = new Dictionary<string, bool>
                    {
                        {"statsInboundUplink", true},
                        {"statsInboundDownlink", true},
                    }
                };
                rootJObj.Add("policy", JObject.FromObject(policy));
                hasChange = true;
            }

            if (!(rootJObj.SelectToken("routing.rules[?(@inboundTag[0] == 'api')]") is JObject))
            {
                JArray rules = (rootJObj.SelectToken("routing.rules") as JArray) ?? new JArray();
                var rule = new
                {
                    inboundTag = new string[] {"api"},
                    outboundTag = "api",
                    type = "field"
                };
                rules.Insert(0,JObject.FromObject(rule));
                hasChange = true;
            }

            if (null == (rootJObj.SelectToken("routing") as JObject).Property("strategy"))
            {
                (rootJObj.SelectToken("routing") as JObject).Add("strategy", "rules");
                hasChange = true;
            }

            RayConfigRepository repo = new RayConfigRepository();
            RayPort statPort = new RayPort
            {
                Listen = "127.0.0.1",
                Port = 13888,
                Protocol = "dokodemo-door",
                Settings = new RayPortSettings {Address = "127.0.0.1"},
                Tag = "api",
                
            };
            repo.AddPort(statPort);

            if(hasChange)
                WriteJsonToFile(rootJObj);
        }

        /// <summary>
        /// 获取用户使用的下行流量
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns>用户的下行流量</returns>
        public long GetDownloadLinkTraffic(RayPortUser user)
        {
            string args = string.Format(
                "api --server=127.0.0.1:13888 StatsService.QueryStats \"pattern: 'user>>>{0}>>>traffic>>>downlink' reset: false\"",
                user.Email);
            string output = RunV2Ctl(
                "/usr/bin/v2ray/v2ctl",
                args);
           
            Regex regex = new Regex("(?<=value: )[0-9]*");
            string traffic = null;
            if (regex.IsMatch(output))
                traffic = regex.Match(output).Value;

            return long.Parse(traffic);
        }

        /// <summary>
        /// 获取用户使用的上行流量
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns>用户的上行流量</returns>
        public long GetUpLinkTraffic(RayPortUser user)
        {
            string output = RunV2Ctl(
                "/usr/bin/v2ray/v2ctl",
                string.Format(
                    "api --server=127.0.0.1:13888 StatsService.QueryStats \"pattern: 'user>>>{0}>>>traffic>>>uplink' reset: false\"",
                    user.Email));

            Regex regex = new Regex("(?<=value: )[0-9]*");
            string traffic = regex.Match(output).Value;

            return long.Parse(traffic);
        }

        /// 获取用户使用的总流量, 包括上行流量和下行流量
        /// </summary>
        /// <param name="user">查询的用户</param>
        /// <returns>用户已使用的流量, 包括上行和下行流量, 单位为B(字节)</returns>
        public long GetTraffic(RayPortUser user)
        {
            return GetDownloadLinkTraffic(user) + GetUpLinkTraffic(user);
        }
    }


    class RayPortEqualityComparer : IEqualityComparer<RayPort>
    {
        public bool Equals([AllowNull] RayPort x, [AllowNull] RayPort y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            return x.Port == y.Port;
        }

        public int GetHashCode([DisallowNull] RayPort obj)
        {
            return obj?.Port.GetHashCode()??GetHashCode();
        }

        public static RayPortEqualityComparer Default => new RayPortEqualityComparer();
    }

    class RayPortUserEqualityComparer : IEqualityComparer<RayPortUser>
    {
        public RayPortUserEqualityComparer()
        {
        }

        public static RayPortUserEqualityComparer Default => new RayPortUserEqualityComparer();

        public bool Equals([AllowNull] RayPortUser x, [AllowNull] RayPortUser y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null) return false;

            return x.Uuid == y.Uuid;
        }

        public int GetHashCode([DisallowNull] RayPortUser obj)
        {
            return obj.Uuid.GetHashCode();
        }
    }
}
