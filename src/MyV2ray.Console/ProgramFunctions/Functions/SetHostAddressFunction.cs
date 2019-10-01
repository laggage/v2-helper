namespace MyV2ray.Console.ProgramFunctions.Functions
{
    using System;
    using MyV2ray.Core;
    using MyV2ray.Core.Database;

    internal class SetHostAddressFunction:IProgramFunction
    {
        public static string FunctionName => "设置本机域名或地址";

        private RayConfigExtensionRepository repo = new RayConfigExtensionRepository();

        public void Execute()
        {
            string address = InputHelper.GetInput("请输入本机ip或域名", "本机ip或域名或用来生成分享链接");
            try
            {
                repo.SetHostAddress(address);
            }
            catch (Exception ex)
            {
                Displayer.ShowError(ex);
            }
            finally
            {
                Displayer.PressAnyKeyToContinue();
            }
        }
    }
}
