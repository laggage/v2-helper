namespace MyV2ray.Core
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class WideWebIpGetter
    {
        public static string GetIp()
        {
            HttpClient client = new HttpClient();
            //client.GetAsync()
            Task<string> t = client.GetStringAsync("http://ip.taobao.com/service/getIpInfo.php?ip=myip");
            t.Wait();
            return t.Result;
        }
    }
}
