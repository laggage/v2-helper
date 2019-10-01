namespace MyV2ray.Console
{
    using System;
    using System.IO;
    using MyV2ray.Console.ProgramFunctions;
    using MyV2ray.Core;
    using MyV2ray.Core.Models;

    class Program
    {
        public const ConsoleColor HighLightColor = ConsoleColor.DarkGreen;
        public const ConsoleColor WarningColor = ConsoleColor.Yellow;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;

        public const string AppConfigFilePath = "./appSettings.json";

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine();
                Displayer.ShowCutLine('=', ErrorColor);
                Displayer.ShowLine($"遇到错误: {s.GetType().Assembly}\r\n  {(e.ExceptionObject as Exception).Message}");
            };
        }

        public Program()
        {
        }

        static void Main(string[] args)
        {
            CheckConfigFilePath();
            Program p = new Program();
            p.Run();
        }

        static void CheckConfigFilePath()
        {
            if (!File.Exists(RayConfig.ConfigFilePath))
            {
                Console.WriteLine();
                Displayer.ShowCutLine('=', ErrorColor);
                Console.WriteLine();
                Displayer.ShowLine($"  无法找到配置文件: {RayConfig.ConfigFilePath}\r\n", ErrorColor);
                Displayer.ShowLine("  应用程序即将退出.\r\n", HighLightColor);
                Environment.Exit(-1);
            }
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    var func = FunctionFactory.GetProgramFunction();
                    func.Execute();
                }
                catch (Exception ex)
                {
                    Displayer.ShowError(ex);
                    Displayer.PressAnyKeyToContinue();
                }
                Console.Clear();
            }
        }
    }
}
