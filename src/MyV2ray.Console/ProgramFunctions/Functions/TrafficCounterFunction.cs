namespace MyV2ray.Console.ProgramFunctions.Functions
{
    using System;
    using MyV2ray.Core;
    using MyV2ray.Core.Database;
    using MyV2ray.Core.Extensions;
    using MyV2ray.Core.Models;

    class TrafficCounterFunction:IProgramFunction
    {
        public static string FunctionName=>"流量统计";

        private readonly RayConfigRepository repo;

        public TrafficCounterFunction()
        {
            repo = new RayConfigRepository();
        }

        /// <summary>
        /// 显示用户流量统计
        /// </summary>
        /// <param name="user"></param>
        private void DisplayUserTrafficStatistics(RayPortUser user)
        {
            Displayer.ShowLine(
                user.GetRayPortUserRemark() + ": ",
                addReturn: true,
                color: ConsoleColor.Yellow);
            Displayer.ShowConfigItem(
                "上行流量",
                repo.GetUpLinkTraffic(user).ToTrafficString(),
                valueColor: Displayer.HighLightColor,
                addReture: true);
            Displayer.ShowConfigItem(
                "下行流量",
                repo.GetDownloadLinkTraffic(user).ToTrafficString(),
                valueColor: Displayer.HighLightColor,
                addReture: true);
            Displayer.ShowConfigItem(
                "已使用总流量",
                repo.GetTraffic(user).ToTrafficString(),
                valueColor: Displayer.HighLightColor,
                addReture: true);
        }

        public void Execute()
        {
            var users = repo.GetRayPortsUsers();
            Console.Clear();
            
            foreach (var user in users)
            {
                try
                {
                    DisplayUserTrafficStatistics(user);
                }
                catch (Exception ex)
                {
                    Displayer.ShowError(ex);
                    continue;
                }
            }
            Displayer.PressAnyKeyToContinue();
        }
    }
}
