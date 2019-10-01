namespace MyV2ray.Console.ProgramFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using MyV2ray.Core;
    using MyV2ray.Core.Database;
    using MyV2ray.Core.Extensions;
    using MyV2ray.Core.Models;

    abstract class RayConfigModifier
    {
        /// <summary>
        /// 索引-修改操作-修改操作描述 字典
        /// </summary>
        protected IDictionary<int, Tuple<string, Action>> optionsMap;

        /// <summary>
        /// 指示是否退出
        /// </summary>
        protected bool exitFunction = false;

        private static readonly IDictionary<int, Type> map = new Dictionary<int, Type>
        {
            {1, typeof(RayPortModifier)},
            {2, typeof(RayPortUserModifier)}
        };

        public static string ModifierName { get; protected set; }

        public abstract void Modify();

        public static RayConfigModifier GetRayConfigModifier(int modifierIndex)
        {
            if (!map.ContainsKey(modifierIndex))
                throw new ArgumentOutOfRangeException(nameof(modifierIndex));
            Type type = map[modifierIndex];
            Assembly assembly = type.Assembly;
            if (!(assembly
                .CreateInstance(type.FullName) is RayConfigModifier modifier))
                throw new Exception($"无法创建{type.FullName}的实例.");
            return modifier;
        }

        public static IEnumerable<string> GetModifiers()
        {
            return map.Values.Select(t =>
            {
                Assembly a = t.Assembly;
                return t.GetField("ModifierName")
                    ?.GetValue(null) as string;
            });
        }

        public static RayConfigModifier GetModifier(int index)
        {
            return map[index].ToInstance<RayConfigModifier>();
        }

        /// <summary>
        /// 显示当前正在修改的项
        /// </summary>
        /// <typeparam name="TValue">正在修改的项的值</typeparam>
        /// <param name="modifyAction">执行修改的操作</param>
        /// <param name="originValue">还未修改的原始值</param>
        protected virtual void ShowModifyTips<TValue>(Action modifyAction, TValue originValue)
        {
            string info = GetModifyActionDesc(modifyAction);

            Displayer.ShowCutLine();
            Console.WriteLine("\r\n");
            Displayer.Show($"修改", 2);
            Displayer.Show($"{info}\r\n");
            Displayer.ShowConfigItem($"原{info}", originValue, valueColor: Displayer.HighLightColor, addReture: true);
        }

        /// <summary>
        /// 从字典中获取修改操作的描述
        /// </summary>
        /// <param name="modifyAction">修改操作</param>
        /// <returns>修改操作的描述</returns>
        protected virtual string GetModifyActionDesc(Action modifyAction)
        {
            return optionsMap
                .FirstOrDefault(m => m.Value.Item2 == modifyAction)
                .Value
                .Item1
                .Split(':').FirstOrDefault();
        }

        protected void FinishedWithoutSave()
        {
            Console.WriteLine();
            //Displayer.ShowLine("即将")
            Displayer.ShowLine("是否要放弃更改并退出?输入回车键确认!", Displayer.ErrorColor);
            var ch = Console.ReadKey();
            if (ch.Key == ConsoleKey.Enter)
                exitFunction = true;
        }

        protected abstract void UpdateOptionsMap();
    }

    abstract class RayConfigModifier<TConfig> : RayConfigModifier
    {
        public TConfig ConfigToModify { get; protected set; }

        static RayConfigModifier()
        {
            ModifierName = "Abstract Modifier";
        }

        public RayConfigModifier()
        {
        }

        public RayConfigModifier(TConfig configToModify)
        {
            ConfigToModify = configToModify;
        }
    }

    class RayPortModifier : RayConfigModifier<RayPort>
    {
        private readonly RayConfigRepository configRepo;
        private readonly int portToModify;

        public new const string ModifierName = "修改端口配置";

        #region Constructor

        static RayPortModifier()
        {
        }


        public RayPortModifier() : this(GetRayPortToModify())
        {
        }

        public RayPortModifier(RayPort rayPortToModify) : base(rayPortToModify)
        {
            configRepo = new RayConfigRepository();
            portToModify = ConfigToModify.Port;
            UpdateOptionsMap();
        }

        #endregion

        private static RayPort GetRayPortToModify()
        {
            RayConfigRepository repo = new RayConfigRepository();

            IList<RayPort> rayPorts = repo.GetRayPorts();

            ConsoleConfigDisplayer.DisplayRayPorts(rayPorts,false, true);

            int? index = InputHelper.TryGetNumberInput(
                             inputRange: new Tuple<int, int>(1, rayPorts.Count)) - 1;

            if (index == null)
            {
                return GetRayPortToModify();
            };

            return rayPorts[index.Value];
        }


        private void FinishModify()
        {
            configRepo.UpdatePort(portToModify, ConfigToModify);

            RayPort rayPortModified = configRepo.GetRayPort(ConfigToModify.Port);

            Displayer.ShowCutLine();
            Displayer.ShowLine("修改成功!",intend:2, addReturn:true);
            ConsoleConfigDisplayer.DisplayRayPort(rayPortModified);
            Displayer.PressAnyKeyToContinue();

            exitFunction = true;
        }

        //protected void ShowMofiyTips<TValue>(Action modifyAction, TValue value)
        //{
        //    string info = GetModifyActionDesc(modifyAction).Split(':').FirstOrDefault();

        //    Displayer.ShowCutLine();
        //    Displayer.Show($"修改", 2);
        //    Displayer.Show($"{info}\r\n\r\n");
        //    Displayer.ShowConfigItem($"原{info}", value);
        //}

        private void ModifyPortTlsCertKeyPath()
        {
            ShowModifyTips(
                ModifyPortTlsCertKeyPath, 
                ConfigToModify?.StreamSettings?
                    .TlsSettings?.Certificates?.FirstOrDefault().KeyFile);
            string newValue = InputHelper.GetInput($"请输入新的{GetModifyActionDesc(ModifyPortTlsCertKeyPath)}");

            IList<Certificate> certs = ((ConfigToModify.StreamSettings ??= new RayPortStreamSettings())
                .TlsSettings ??= new TlsSettings())
                .Certificates ??= new List<Certificate>();
            //certs.Clear();
            Certificate c = certs.FirstOrDefault();
            certs.Clear();
            c.KeyFile = newValue;
            certs.Add(c);
        }

        private void ModifyPortTlsCertPath()
        {
            ShowModifyTips(
                ModifyPortTlsCertPath, 
                ConfigToModify?.StreamSettings?
                    .TlsSettings?.Certificates?
                    .FirstOrDefault().CertificateFile);
            string newValue = InputHelper.GetInput($"请输入新的{GetModifyActionDesc(ModifyPortTlsCertPath)}");

            IList<Certificate> certs = ((ConfigToModify.StreamSettings ??= new RayPortStreamSettings())
                    .TlsSettings ??= new TlsSettings())
                .Certificates ??= new List<Certificate>();

            Certificate c = certs.FirstOrDefault();
            certs.Clear();
            c.CertificateFile = newValue;
            certs.Add(c);
        }

        private void ModifyPortSecurity()
        {
            Action action = ModifyPortSecurity;

            ShowModifyTips(action, ConfigToModify.StreamSettings?.Security);

            string newValue = InputHelper.GetInput(
                $"请输入新的{GetModifyActionDesc(action)}");

            string originValue = ConfigToModify.StreamSettings?.Security;

            if (newValue != originValue)
                (ConfigToModify.StreamSettings
                        ??= new RayPortStreamSettings())
                    .Security = newValue;
        }

        private void ModifyPortNetwork()
        {
            ShowModifyTips(ModifyPortNetwork, ConfigToModify?.StreamSettings?.NetWork);

            string newValue = InputHelper.GetInput($"请输入新的{GetModifyActionDesc(ModifyPortNetwork)}");
            string originValue = ConfigToModify.StreamSettings?.NetWork;

            if (newValue != originValue)
                (ConfigToModify.StreamSettings ??= new RayPortStreamSettings()).NetWork = newValue;
        }

        private void ModifyConnectionReuse()
        {
            Action action = ModifyConnectionReuse;
            bool? originValue = ConfigToModify.StreamSettings?.WSSettings?.ConnectionReuse;

            ShowModifyTips(action, ConfigToModify.Port);
            bool? newValue = null;
            try
            {
                newValue = bool.Parse(InputHelper.GetInput(
                    $"请输入新的{GetModifyActionDesc(action)}"));
            }
            catch { }

            if (newValue != originValue)
                ((ConfigToModify.StreamSettings ??= new RayPortStreamSettings())
                    .WSSettings ??= new WSSettings()).ConnectionReuse = newValue.Value;
        }

        private void ModifyPortNumber()
        {
            Action action = ModifyPortNumber;
            int originValue = ConfigToModify.Port;

            ShowModifyTips(action, ConfigToModify.Port);

            int? newValue = InputHelper.TryGetNumberInput(
                $"请输入新的{GetModifyActionDesc(action)}");

            if (newValue == null) return;

            if (newValue != originValue)
                ConfigToModify.Port = newValue.Value;
        }

        private void ShowMofiyOptions()
        {
            IEnumerable<string> options = optionsMap.Values.Select(v => v.Item1);
            Console.Clear();
            Displayer.ShowOptions(
                hasEmptyLineAfterEachOption: true, options: options.ToArray());
        }

        public override void Modify()
        {
            ShowMofiyOptions();

            int? ch = InputHelper.TryGetNumberInput();

            if (ch == null || !optionsMap.ContainsKey(ch.Value))
            {
                Displayer.PressAnyKeyToContinue("输入格式错误!", Displayer.ErrorColor);
                Modify();
            }
            else
            {
                optionsMap[ch.Value].Item2.Invoke();

                if (exitFunction == true)
                    return;

                UpdateOptionsMap();
                Modify();
            }
        }

        private void ModifyWsPath()
        {
            Action action = ModifyWsPath;

            ShowModifyTips(action, ConfigToModify?.StreamSettings?.WSSettings?.Path);

            string newValue = InputHelper.GetInput($"请输入新的{GetModifyActionDesc(action)}");
            string originValue = ConfigToModify.StreamSettings?.NetWork;

            ((ConfigToModify.StreamSettings ??= new RayPortStreamSettings())
                .WSSettings ??= new WSSettings()).Path = newValue;
        }

        private void ModifyPortListenAddress()
        {
            Action action = ModifyPortListenAddress;

            ShowModifyTips(action, ConfigToModify.Listen);

            string newValue = InputHelper.GetInput($"请输入新的{GetModifyActionDesc(action)}");
            string originValue = ConfigToModify.StreamSettings?.NetWork;

            ConfigToModify.Listen = newValue;
        }

        protected override void UpdateOptionsMap()
        {
            int index = 1;
            optionsMap = new Dictionary<int, Tuple<string, Action>>
            {
                {index++, new Tuple<string, Action>($"保存退出",FinishModify) },
                {index++, new Tuple<string, Action>($"直接退出",FinishedWithoutSave) },
                {index++, new Tuple<string, Action>($"端口号: {ConfigToModify?.Port}",ModifyPortNumber)  },
                {index++, new Tuple<string, Action>($"传输协议: {ConfigToModify?.StreamSettings?.NetWork}",ModifyPortNetwork)  },
                {index++, new Tuple<string, Action>($"连接复用: {ConfigToModify?.StreamSettings?.WSSettings?.ConnectionReuse}",ModifyConnectionReuse) },
                {index++, new Tuple<string, Action>($"底层传输安全: {ConfigToModify?.StreamSettings?.Security}",ModifyPortSecurity) },
                {index++, new Tuple<string, Action>($"Ws路径(path): {ConfigToModify?.StreamSettings?.WSSettings?.Path}",ModifyWsPath) },
                {index++, new Tuple<string, Action>($"端口监听地址: {ConfigToModify?.Listen}",ModifyPortListenAddress) },
                {index++, new Tuple<string, Action>($"tls证书文件路径: {ConfigToModify?.StreamSettings?.TlsSettings?.Certificates?.FirstOrDefault().CertificateFile}",ModifyPortTlsCertPath) },
                {index++, new Tuple<string, Action>($"tls密钥文件路径: {ConfigToModify?.StreamSettings?.TlsSettings?.Certificates?.FirstOrDefault().KeyFile}",ModifyPortTlsCertKeyPath) },
            };
        }
    }

    class RayPortUserModifier : RayConfigModifier<RayPortUser>
    {
        private object syncRoot;
        public new const string ModifierName = "修改用户信息";

        private RayConfigRepository configRepo;
        private RayConfigRepository ConfigRepo
        {
            get
            {
                if (configRepo == null)
                {
                    lock (syncRoot)
                        if (configRepo == null)
                            configRepo = new RayConfigRepository();
                }

                return configRepo;
            }
        }

        #region Constructor

        static RayPortUserModifier()
        {
        }

        public RayPortUserModifier() : base(GetRayPortUserToModify())
        {

            UpdateOptionsMap();

            syncRoot = new object();
        }

        public RayPortUserModifier(RayPortUser rayPortUser) : base(rayPortUser)
        {
        }

        #endregion

        protected override void UpdateOptionsMap()
        {
            int index = 1;
            optionsMap = new Dictionary<int, Tuple<string, Action>>
            {
                {index++, new Tuple<string, Action>($"保存退出", ModifyFinished) },
                {index++, new Tuple<string, Action>($"直接退出", FinishedWithoutSave) },
                {index++, new Tuple<string, Action>($"用户名: {ConfigToModify.GetRayPortUserRemark()}", ModifyUserEmail)},
                {index++, new Tuple<string, Action>($"额外ID: {ConfigToModify.AlterId}", ModifyUserAlterID)},
                {index++, new Tuple<string, Action>($"用户等级(Level): {ConfigToModify.Level}", ModifyUserLevel)},
                {index++, new Tuple<string, Action>($"注册日期: ", ModifyUserRegistrationDate) },
            };
        }

        private static RayPortUser GetRayPortUserToModify()
        {
            var repo = new RayConfigRepository();
            var rayPortsUsers = repo.GetRayPortsUsers();

            ConsoleConfigDisplayer.DisplayRayPortUsers(
                rayPortsUsers, displayUserIndex:true, intend:2);

            int? index = InputHelper.TryGetNumberInput(
                    inputRange: new Tuple<int, int>(1, rayPortsUsers.Count)) - 1;

            if (index == null)
            {
                return GetRayPortUserToModify();
            };

            return rayPortsUsers[index.Value];
        }

        private void ModifyUserEmail()
        {
            Action action = ModifyUserEmail;
            string orginValue = ConfigToModify.GetRayPortUserRemark();

            ShowModifyTips(action, orginValue);

            string newValue = InputHelper.GetInput($"请输入新的{GetModifyActionDesc(ModifyUserEmail)}(不要包含字符'@')");

            if (newValue != orginValue)
                ConfigToModify.Email = newValue+RayPortUser.EmailSuffix;
        }

        /// <summary>
        /// 修改用户注册时间
        /// </summary>
        private void ModifyUserRegistrationDate()
        {
            throw new NotImplementedException();
        }

        private void ModifyUserLevel()
        {
            Action action = ModifyUserLevel;
            int orginValue = ConfigToModify.Level;

            ShowModifyTips(action, orginValue);

            int? newValue = InputHelper.TryGetNumberInput($"请输入新的{GetModifyActionDesc(action)}");

            if (newValue == null)
            {
                Displayer.PressAnyKeyToContinue();
                return;
            }

            if (newValue.Value != orginValue)
                ConfigToModify.Level = newValue.Value;
        }

        private void ModifyUserAlterID()
        {
            Action action = ModifyUserAlterID;
            int orginValue = ConfigToModify.AlterId;

            ShowModifyTips(action, orginValue);

            int? newValue = InputHelper.TryGetNumberInput($"请输入新的{GetModifyActionDesc(action)}");

            if (newValue == null)
            {
                Displayer.PressAnyKeyToContinue();
                return;
            }

            if (newValue.Value != orginValue)
                ConfigToModify.AlterId = newValue.Value;
        }

        private void ModifyFinished()
        {
            ConfigRepo.UpdateUser(ConfigToModify);

            Displayer.ShowLine("修改完成!\r\n", 2);
            Displayer.PressAnyKeyToContinue();

            exitFunction = true;
        }

        private void ShowModifyOptions()
        {
            var options = optionsMap.Select(m => m.Value.Item1);
            Displayer.ShowOptions(indexColor:Displayer.HighLightColor,
                options: options.ToArray());
        }

        public override void Modify()
        {
            if (ConfigToModify == null) return;

            Console.Clear();

            ShowModifyOptions();

            int? ch = InputHelper.TryGetNumberInput("请输入序号来选择要修改的用户信息项");
            Displayer.ShowConfigItem("你输入了", ch?.ToString()??string.Empty, valueColor: Displayer.HighLightColor, addReture: true);

            if (ch == null || !optionsMap.Keys.Contains(ch.Value))
            {
                Displayer.PressAnyKeyToContinue("输入格式错误!", Displayer.ErrorColor);
                Modify();
            }
            else
            {
                optionsMap[ch.Value].Item2.Invoke();

                if (exitFunction == true)
                    return;

                UpdateOptionsMap();
                Modify();
            }
        }
    }
}
