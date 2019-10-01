namespace MyV2ray.Console.ProgramFunctions.Functions
{
    using System;
    using MyV2ray.Core;
    using MyV2ray.Core.Database;

    class ConfigTrafficStatisticFunction:IProgramFunction
    {
        public static string FunctionName => "自动配置流量统计功能";

        public void Execute()
        {
            try
            {
                RayConfigRepository.ConfigTrafficStatistic();
                IProgramFunction func = new ShowRawConfigFunction();
                func.Execute();
            }
            catch (Exception ex)
            {
                Displayer.ShowError(ex);
            }
        }
    }
}
