namespace MyV2ray.Core.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Text.RegularExpressions;
    using MyV2ray.Core.Database;
    using MyV2ray.Core.Models;

    public static class RayConfigExtensions
    {
        /// <summary>
        /// 生成 V2ray 分享链接
        /// </summary>
        /// <param name="rayPortUser"></param>
        /// <param name="rayPort"></param>
        /// <returns></returns>
        public static string GenerateShareUrl(this RayPortUser rayPortUser, RayPort rayPort)
        {
            
            ClientUserConfig clientConfig = new ClientUserConfig(rayPort, rayPortUser);
            string jStr = clientConfig.ToJsonString(RayConfigJsonSetting.JsonSerializerSettings);
            return "vmess://" + Convert.ToBase64String(Encoding.Default.GetBytes(jStr));
        }

        public static string GetRayPortUserRemark(this RayPortUser rayPortUser)
        {
            // rayPortUser.Email.Split('@')
            Regex regex = new Regex("^.*(?=@)");
            if (string.IsNullOrEmpty(rayPortUser.Email))
            {
                rayPortUser.Email = (new Random()).Next(1, int.MaxValue).ToString() + "@la.aggage";
                RayConfigRepository.UpdateUser(rayPortUser.Uuid, rayPortUser);
            }
                
            return regex.Match(rayPortUser.Email)?.Value ?? "";
        }

        public static string QueryTraffic(this RayPortUser user)
        {
            var psi = new ProcessStartInfo("dotnet", "--info")
            {
                RedirectStandardOutput = true
            };

            var proc = Process.Start(psi);
            if (proc == null)
            {
                Displayer.ShowLine("Failed to query traffic");
            }
            else
            {
                using var sr = proc.StandardOutput;
                string text = sr.ReadToEnd();
            }
            throw new NotImplementedException();
        }
    }
}
