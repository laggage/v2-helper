namespace MyV2ray.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MyV2ray.Core;
    using MyV2ray.Core.Database;
    using MyV2ray.Core.Extensions;
    using MyV2ray.Core.Models;

    internal class ConsoleConfigDisplayer
    {
        public static void DisplayRayPort(RayPort rayPort,
            int? index = null,bool displayUser = false,bool displayUserIndex = false, bool addReturn = true)
        {
            Displayer.ShowConfigItem("索引", index, ConsoleColor.Green);
            Displayer.ShowConfigItem("端口号", rayPort.Port);
            Displayer.ShowConfigItem("服务类型", rayPort.Protocol);
            Displayer.ShowConfigItem("传输协议", rayPort.StreamSettings?.NetWork??"tcp");
            Displayer.ShowConfigItem("底层传输安全", rayPort.StreamSettings?.Security ?? "");
            Displayer.ShowConfigItem("Ws路径(path)", rayPort?.StreamSettings?.WSSettings?.Path);
            Displayer.ShowConfigItem("端口监听地址", rayPort?.Listen);
            Displayer.ShowConfigItem("tls证书文件路径", rayPort?.StreamSettings?.TlsSettings?.Certificates?.FirstOrDefault().CertificateFile);
            Displayer.ShowConfigItem("tls证书文件路径", rayPort?.StreamSettings?.TlsSettings?.Certificates?.FirstOrDefault().KeyFile);

            if (displayUser)
            {
                Console.WriteLine();
                DisplayRayPortUsers(rayPort.Settings?.Clients, displayUserIndex, intend:4);
            }

            if (addReturn) Console.WriteLine();
        }

        public static void DisplayRayPorts(IEnumerable<RayPort> rayPorts, bool displayUsers = true)
        {
            int i = 0;
            foreach (RayPort rayPort in rayPorts)
                DisplayRayPort(rayPort, ++i, displayUser:displayUsers);
        }

        public static void DisplayRayPorts(
            IEnumerable<RayPort> rayPorts, 
            bool displayUserIndex, bool displayUsers = true)
        {
            int? i = 0;
            foreach (RayPort rayPort in rayPorts)
                DisplayRayPort(rayPort, ++i ,  displayUser: displayUsers, displayUserIndex:displayUserIndex);
        }

        public static void DisplayUser(
            RayPortUser rayPortUser, int? index = null, 
            bool showShareUrl = true, bool addReturn = true,
            int intend = 2)
        {
            if (rayPortUser == null) return;
            StringBuilder s = new StringBuilder("");
            StringWriter sw = new StringWriter(s);
            s.Append(' ', intend);
            Displayer.ShowConfigItem(s+"索引", index, Program.HighLightColor);
            Displayer.ShowConfigItem(s+"用户", rayPortUser.GetRayPortUserRemark(), Program.WarningColor);
            Displayer.ShowConfigItem(s+"用户Id", rayPortUser.Uuid);
            Displayer.ShowConfigItem(s+"用户额外Id", rayPortUser.AlterId);
            Displayer.ShowConfigItem(s+"等级(Level)", rayPortUser.Level);
            Displayer.ShowConfigItem(s+"邮箱", rayPortUser.Email);
            if(showShareUrl)
                try
                {
                    Displayer.ShowConfigItem(
                        "分享链接",
                        rayPortUser.GenerateShareUrl(RayConfigRepository.GetRayPort(rayPortUser)), Program.ErrorColor);
                }
                catch (Exception) { }
                
            if(addReturn) Console.WriteLine();
        }

        internal static void Display()
        {
            RayConfigRepository rayConfigRepo = new RayConfigRepository();
            IList<RayPort> rayPorts = rayConfigRepo.GetRayPorts();
            Console.WriteLine("\r\n");
            
            foreach (RayPort rayPort in rayPorts)
            {
                Displayer.ShowConfigItem("端口号", rayPort.Port, true, valueColor:ConsoleColor.Green);
                Displayer.ShowConfigItem("服务类型", rayPort.Protocol, true);
                Displayer.ShowConfigItem("是否Udp开启", rayPort.Settings.Udp, true);
                Displayer.ShowConfigItem("底层传输安全", rayPort.StreamSettings?.Security, true);
                Displayer.ShowConfigItem("传输协议", rayPort.StreamSettings?.NetWork, true);

                foreach (RayPortUser client in rayPort.Settings?.Clients ?? new List<RayPortUser>())
                {
                    Displayer.ShowConfigItem("用户", client.GetRayPortUserRemark(), ConsoleColor.Yellow);
                    Displayer.ShowConfigItem("用户Id", client.Uuid, ConsoleColor.Yellow);
                    Displayer.ShowConfigItem("额外Id", client.AlterId, ConsoleColor.Yellow);
                    Displayer.ShowConfigItem("邮箱", client.Email, ConsoleColor.Yellow);
                    Displayer.ShowConfigItem("分享链接", client.GenerateShareUrl(rayPort), ConsoleColor.Red,true);
                }
                
                Console.WriteLine();

                foreach (var cert in rayPort.StreamSettings?.TlsSettings?.Certificates ?? new List<Certificate>())
                {
                    Displayer.ShowConfigItem("证书文件", cert.CertificateFile);
                    Displayer.ShowConfigItem("密钥文件", cert.KeyFile);
                    Console.WriteLine();
                }

                Displayer.ShowCutLine();
                Console.WriteLine("\r\n");
            }

            Displayer.Show("  共开放了端口 ");
            Displayer.Show($"{rayPorts.Count}", Displayer.HighLightColor);
            Displayer.Show(" 个, ", Displayer.HighLightColor);
            Displayer.Show("有 ");
            Displayer.Show($"{rayConfigRepo.GetRayPortsUsers().Count}", Displayer.HighLightColor);
            Displayer.Show(" 个用户\r\n\r\n");
        }

        internal static void DisplayRayPortUsers(IList<RayPortUser> rayPortUsers, bool addReturn = true, bool displayUserIndex = false, int intend = 4)
        {
            if (rayPortUsers == null) return;

            int? index = 0;
            foreach (var rayPortUser in rayPortUsers)
                DisplayUser(rayPortUser, displayUserIndex?++index:null, intend:intend);
            if (addReturn) Console.WriteLine();
        }
    }
}
