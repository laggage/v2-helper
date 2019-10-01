namespace MyV2ray.Console.ProgramFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MyV2ray.Core;
    using MyV2ray.Console.ProgramFunctions.Functions;
    using MyV2ray.Core.Database;
    using MyV2ray.Core.Extensions;
    using MyV2ray.Core.Models;

    class CheckRayConfigFunction: IProgramFunction
    {
        public static string FunctionName => "查看配置";

        public void Execute()
        {
            Console.Clear();
            ConsoleConfigDisplayer.Display();
            Displayer.PressAnyKeyToContinue();
        }
    }

    class ModifyRayConfigFunction : IProgramFunction
    {
        public static string FunctionName => "修改配置";

        public void Execute()
        {
            string[] configModifyOptions = RayConfigModifier.GetModifiers().ToArray();

            Displayer.ShowOptions(Displayer.HighLightColor, options: configModifyOptions);

            int? ch = InputHelper.TryGetNumberInput();
            if (ch != null)
            {
                RayConfigModifier.GetRayConfigModifier(ch.Value)
                    .Modify();
            }
        }
    }

    class CreateRayPortFunction : IProgramFunction
    {
        public static string FunctionName => "新建端口";

        public void Execute()
        {
            Console.Clear();
            Console.WriteLine();
            Displayer.ShowLine("新建端口向导!",2, ConsoleColor.Yellow);
            Console.WriteLine();
            Displayer.ShowCutLine();
            Console.WriteLine("\r\n");
            ConsoleInputRayPortConfigBuilder rayPortBuilder = new ConsoleInputRayPortConfigBuilder();
            try
            {
                RayConfigRepository repo = new RayConfigRepository();
                RayPort rayPort = rayPortBuilder.BuildPort();
                repo.AddPort(rayPort);
                Displayer.ShowLine("  创建成功!\r\n", ConsoleColor.DarkGreen);
                ConsoleConfigDisplayer.DisplayRayPort(rayPort, displayUser: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    class CreateRayPortUserFunction : IProgramFunction
    {
        public static string FunctionName => "新建用户";

        public void Execute()
        {
            RayConfigRepository repo = new RayConfigRepository();

            IList<RayPort> ports = repo.GetRayPorts();
            ConsoleConfigDisplayer.DisplayRayPorts(ports);
            try
            {
                int ch = InputHelper.GetNumberInput("输入端口索引选择端口", tipsColor: ConsoleColor.DarkGreen);
                ch -= 1;
                if (ch >= ports.Count)
                {
                    Displayer.ShowLine("输入错误, 超出索引.", ConsoleColor.Red);
                    return;
                }

                ConsoleInputRayPortConfigBuilder rayPortBuilder =
                    new ConsoleInputRayPortConfigBuilder(ports[ch]);
                RayPortUser user = rayPortBuilder.BuildPortUser();
                repo.AddUserToPort(ports[ch], user);

                Displayer.ShowLine("操作成功!", ConsoleColor.DarkGreen, true);
                ConsoleConfigDisplayer.DisplayUser(user);
            }
            catch (Exception ex)
            {
                Displayer.ShowLine($"操作失败!\r\n错误:{ex.Message}", ConsoleColor.Red, true);
            }
        }
    }

    class DeleteRayPortFunction : IProgramFunction
    {
        public static string FunctionName => "删除端口";

        private readonly RayConfigRepository repo;
        private RayPort rayPortToDelete;

        public DeleteRayPortFunction()
        {
            repo = new RayConfigRepository();
        }

        private void ShowRayPortsAndGetRayPortToDelete()
        {
            IList<RayPort> rayPorts = repo.GetRayPorts();

            ConsoleConfigDisplayer.DisplayRayPorts(rayPorts, false);

            int? ch = InputHelper.TryGetNumberInput(
                "输入索引选择要删除的端口", "删除端口将删除端口下的全部用户", 
                new Tuple<int, int>(1, rayPorts.Count));

            if (ch == null)
            {
                throw new Exception("输入有误");
            }

            rayPortToDelete = rayPorts[ch.Value - 1];
        }

        public void Execute()
        {
            try
            {
                ShowRayPortsAndGetRayPortToDelete();
                repo.DeleteRayPort(rayPortToDelete);
                Displayer.ShowLine("删除成功!", Displayer.HighLightColor);
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

    class DeleteRayPortUserFunction : IProgramFunction
    {
        public static string FunctionName => "删除用户";

        private RayPortUser rayPortUserToDelete;
        private RayConfigRepository repo;

        public DeleteRayPortUserFunction()
        {
            repo = new RayConfigRepository();
        }

        private void ShowRayPortsUsersAndGetRayPortUserToDelete()
        {
            IList<RayPortUser> rayPortsUsers = repo.GetRayPortsUsers();

            Console.Clear();

            ConsoleConfigDisplayer.DisplayRayPortUsers(
                rayPortsUsers, displayUserIndex: true, intend: 2);

            int? ch = InputHelper.TryGetNumberInput(
                "输入索引来选择要删除的用户", inputRange:new Tuple<int, int>(1, rayPortsUsers.Count));

            if (ch == null)
                throw new Exception($"输入错误, 输入范围必须是1-{rayPortsUsers.Count}的数字");

            rayPortUserToDelete = rayPortsUsers[ch.Value-1];
        }

        public void Execute()
        {
            try
            {
                ShowRayPortsUsersAndGetRayPortUserToDelete();
                repo.DeleteRayPortUser(rayPortUserToDelete);
                Displayer.ShowLine("删除成功!", Displayer.HighLightColor);
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

    class ShowRawConfigFunction : IProgramFunction
    {
        public static string FunctionName => "显示原始配置文件";

        public void Execute()
        {
            Console.Clear();
            Displayer.ShowCutLine();
            Console.WriteLine();
            Displayer.ShowLine(RayConfig.RayConfigJObject.ToString(), addReturn:true);
            Displayer.ShowCutLine();
            Displayer.PressAnyKeyToContinue();
        }
    }

    class ExitProgramFunction : IProgramFunction
    {
        public static string FunctionName => "退出程序";

        public void Execute()
        {
            Environment.Exit(0);
        }
    }

    class FunctionFactory
    {
        private static readonly string[] appFunctions =
        {
            ExitProgramFunction.FunctionName,
            SetHostAddressFunction.FunctionName,
            TrafficCounterFunction.FunctionName,
        };
        private static readonly string[] rayConfigFunctions =
        {
            CheckRayConfigFunction.FunctionName,
            ModifyRayConfigFunction.FunctionName,
            CreateRayPortFunction.FunctionName,
            CreateRayPortUserFunction.FunctionName,
            DeleteRayPortFunction.FunctionName,
            DeleteRayPortUserFunction.FunctionName,
            ShowRawConfigFunction.FunctionName,
        };

        private static Type ShowFunctionsAndGetFunction()
        {
            List<string> options = new List<string>(appFunctions);
            options.AddRange(rayConfigFunctions);

            Type funcInterface = typeof(IProgramFunction);
            var funcs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()
                    .Where(t => t.GetInterfaces()
                        .Contains(typeof(IProgramFunction))));

            Console.WriteLine();
            Displayer.ShowConfigItem(
                "本机ip或域名", RayConfigExtensionRepository.Create().GetHostAddress(),
                addReture:true, color:ConsoleColor.Yellow);

            Displayer.ShowOptions(Displayer.HighLightColor, ConsoleColor.White, false, appFunctions, rayConfigFunctions);

            try
            {
                int ch = InputHelper.GetNumberInput();
                string funcName = options[ch - 1];
                Type func = funcs.FirstOrDefault(
                    t => (t.GetProperty(nameof(IProgramFunction.FunctionName))
                             .GetValue(null, null) as string) == funcName);
                return func;
            }
            catch
            {
                throw new IndexOutOfRangeException(
                    $"输入错误, 输入的必须是 1-{options.Count} 之间的整数.");
            }
        }

        public static IProgramFunction GetProgramFunction()
        {
            try
            {
                Type funcType = ShowFunctionsAndGetFunction();
                return funcType.ToInstance<IProgramFunction>();
            }
            catch (IndexOutOfRangeException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new InvalidProgramException("无法获得某个应用程序功能的实例",ex);
            }
        }

        //public static IProgramFunction CreateFunction(int funcIndex)
        //{
        //    if (!funcsMap.ContainsKey(funcIndex))
        //        throw new ArgumentOutOfRangeException($"索引 {funcIndex} 没有对应的功能!");
        //    Type funcType = funcsMap[funcIndex];
        //    Assembly assembly = funcType.Assembly;

        //    if (!(assembly.CreateInstance(funcType.FullName) is IProgramFunction programFunction))
        //        throw new Exception($"功能{funcType}初始化失败");

        //    return programFunction;
        //}

        //public static void ExecuteFunction(int funcIndex)
        //{
        //    IProgramFunction function = CreateFunction(funcIndex);
        //    function.Execute();
        //}
    }
}
