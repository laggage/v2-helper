namespace MyV2ray.Core
{
    using MyV2ray.Core.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IRayPortConfigBuilder
    {
        IRayPortConfigBuilder BuildRayPort();
        IRayPortConfigBuilder BuildClients();
        IRayPortConfigBuilder BuildTlsSettings();
        IRayPortConfigBuilder BuildRayPortSettings();
        IRayPortConfigBuilder BuildRayPortStreamSettings();

        RayPort BuildPort();
        RayPortUser BuildPortUser();
    }

    public class ConsoleInputRayPortConfigBuilder: IRayPortConfigBuilder
    {
        private RayPort rayPort;

        private RayPort RayPort => rayPort ??= new RayPort();

        public ConsoleInputRayPortConfigBuilder()
        {
        }

        public ConsoleInputRayPortConfigBuilder(RayPort rayPort)
        {
            this.rayPort = rayPort;
        }

        public IRayPortConfigBuilder BuildClients()
        {
            RayPort.Settings = RayPort.Settings ?? new RayPortSettings();
            RayPort.Settings.Clients = RayPort.Settings.Clients ?? new List<RayPortUser>();

            RayPortUser client = new RayPortUser();
            RayPort.Settings.Clients.Add(client);

            try
            {
                client.Email = InputHelper.GetInput("用户名", "不能包含 @ 字符");
                Displayer.ShowLine("设置成功! " + client.Email + "\r\n", ConsoleColor.DarkGreen);

                client.Email += "@t.tt";

                client.Uuid = InputHelper.GetInput("用户id", "默认 - " + client.Uuid);
                Displayer.ShowLine("设置成功! " + client.Uuid + "\r\n", ConsoleColor.DarkGreen);

                client.Uuid = string.IsNullOrEmpty(client.Uuid)
                    ? Guid.NewGuid().ToString()
                    : client.Uuid;


                client.AlterId = InputHelper.GetNumberInput("额外id", "默认 - " + client.AlterId.ToString());
            }
            catch
            {
                client.Uuid = Guid.NewGuid().ToString();
                client.AlterId = 64;
            }
            finally
            {
                Displayer.ShowLine("设置成功! " + client.AlterId + "\r\n", ConsoleColor.DarkGreen);
            }

            return this;
        }
            
        public IRayPortConfigBuilder BuildTlsSettings()
        {
            throw new NotImplementedException();
        }

        public IRayPortConfigBuilder BuildRayPortSettings()
        {
            RayPort.Settings = RayPort.Settings ?? new RayPortSettings();

            BuildClients();
            return this;
        }

        public IRayPortConfigBuilder BuildRayPortStreamSettings()
        {
            RayPort.StreamSettings = RayPort.StreamSettings ?? new RayPortStreamSettings();
            
            RayPort.StreamSettings.NetWork = InputHelper.GetInput("传输协议", "tcp/kcp/ws/h2/quic, default: tcp");
            Displayer.ShowLine("设置成功! " + RayPort.StreamSettings.NetWork + "\r\n", ConsoleColor.DarkGreen);

            RayPort.StreamSettings.Security = InputHelper.GetInput("底层传输安全", "tls|\"\"");
            Displayer.ShowLine("设置成功! " + RayPort.StreamSettings.Security + "\r\n", ConsoleColor.DarkGreen);

            if (RayPort.StreamSettings.Security == string.Empty) RayPort.StreamSettings.Security = null;

            string path = InputHelper.GetInput("tls路径");
            Displayer.ShowLine("设置成功! " + path + "\r\n", ConsoleColor.DarkGreen);

            string connectionReUse = InputHelper.GetInput("连接复用", "true");
            Displayer.ShowLine("设置成功! " + connectionReUse + "\r\n", ConsoleColor.DarkGreen);

            bool reUse = false;
            if (!string.IsNullOrEmpty(connectionReUse) 
                || bool.TryParse(connectionReUse, out reUse))
            {
                RayPort.StreamSettings.WSSettings = new WSSettings
                {
                    ConnectionReuse = reUse,
                    Path = path
                };
            }

            return this;
        }

        public IRayPortConfigBuilder BuildRayPort()
        {
            RayPort.Port = 0;
            try
            {
                RayPort.Port = InputHelper.GetNumberInput("端口号(0-65535)");
            }
            catch
            {
                RayPort.Port = (new Random()).Next(1, 65535);
            }
            finally
            {
                Displayer.ShowLine("设置成功! " + RayPort.Port + "\r\n", ConsoleColor.DarkGreen);
            }

            try
            {
                string type = InputHelper.GetInput("服务类型", "default - vmess");
                RayPort.Protocol = string.IsNullOrEmpty(type) ? RayPort.Protocol : type;
                Displayer.ShowLine("设置成功! " + RayPort.Protocol + "\r\n", ConsoleColor.DarkGreen);
                BuildRayPortStreamSettings();
            }
            catch (Exception)
            {
                Displayer.ShowLine("设置成功! " + RayPort.Port + "\r\n", ConsoleColor.DarkGreen);
            }

            RayPort.Tag = RayPort.StreamSettings?.NetWork ?? "tcp";
            return this;
        }

        public RayPort BuildPort()
        {
            BuildRayPort().BuildClients();
               
            return rayPort;
        }

        public RayPortUser BuildPortUser()
        {
            Displayer.ShowLine("创建用户\r\n", ConsoleColor.DarkGreen);
            BuildRayPortSettings();
              
            return rayPort.Settings.Clients.LastOrDefault();
        }
    }
}
