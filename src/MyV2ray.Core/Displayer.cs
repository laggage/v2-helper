namespace MyV2ray.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Displayer
    {
        public const ConsoleColor HighLightColor = ConsoleColor.Green;
        public const ConsoleColor WarningColor = ConsoleColor.Yellow;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;

        public static void ShowLine(string text, ConsoleColor color = ConsoleColor.White, bool addReturn = false)
        {
            ShowLine(text, 2 , color, addReturn);
        }

        public static void ShowLine(string text, int? intend, ConsoleColor color = ConsoleColor.White, bool addReturn = false)
        {
            ConsoleColor origin = Console.ForegroundColor;
            Console.ForegroundColor = color;

            StringBuilder sb = new StringBuilder(string.Empty);
            if (intend != null)
            {
                sb.Append(' ', intend.Value);
            }

            Console.Write(sb.ToString()+text + "\r\n");

            Console.ForegroundColor = origin;

            if (addReturn) Console.WriteLine();
        }

        public static void ShowConfigItem<TValue>(
            string title, TValue value, bool addReture = false,
            ConsoleColor titleColor = ConsoleColor.White,
            ConsoleColor valueColor = ConsoleColor.White)
        {
            // 值为 null 或者空则不显示
            if (value == null) return;

            Show("  " + title + ": ", titleColor);
            Show(value.ToString(), valueColor);
            Show("\r\n");
            if (addReture) Show("\r\n");
        }

        [Obsolete]
        public static void ShowOption(string str, ref int index, bool showIndex = true, bool indexAutoIncrement = true,
            ConsoleColor indexColor = ConsoleColor.White,
            ConsoleColor textColor = ConsoleColor.White)
        {
            if (showIndex)
            {
                Show($"{index}.", indexColor);
                if (indexAutoIncrement) ++index;
            }
            Show($"{str}", textColor);
        }

        public static void ShowOptions(ConsoleColor indexColor = ConsoleColor.White,
            ConsoleColor textColor = ConsoleColor.White, bool hasEmptyLineAfterEachOption = false,
            params string[] options)
        {
            ShowLine("-------------------- ",null, textColor);
            Console.WriteLine();
            for (int i = 0; i < options.Length; i++)
            {
                Show($"  {i + 1}", indexColor);
                Show($".{options[i]}\r\n", textColor);
                if (hasEmptyLineAfterEachOption)
                    Console.WriteLine();
            }
            Console.WriteLine();
            ShowLine("-------------------- \r\n", null, textColor);
        }

        public static void ShowOptions(ConsoleColor indexColor = ConsoleColor.White,
            ConsoleColor textColor = ConsoleColor.White, bool hasEmptyLineAfterEachOption = false,
            params IEnumerable<string>[] options)
        {
            ShowLine("-------------------- ", null, textColor);
            Console.WriteLine();
            int count = 0;
            foreach(var o in options)
            {
                for (int i = 0; i < o.Count(); i++)
                {
                    Show($"  {count++ + 1}", indexColor);
                    Show($".{o.ElementAt(i)}\r\n", textColor);  
                    if (hasEmptyLineAfterEachOption)
                        Console.WriteLine();
                } 
                Console.WriteLine();    
            } 
            //Console.WriteLine();
            ShowLine("-------------------- \r\n", null, textColor);
        }

        public static void ShowConfigItem<TValue>(
            string title, TValue value,
            ConsoleColor color, bool addReture = false)
        {
            ShowConfigItem(title, value, addReture, color, color);
        }

        public static void ShowLineIfNotNullOrEmpty(string text, ConsoleColor color = ConsoleColor.White)
        {
            if (!string.IsNullOrEmpty(text))
                ShowLine(text, color);
        }

        public static void Show(string text, ConsoleColor color = ConsoleColor.White)
        {
            ConsoleColor origin = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = origin;
        }

        public static void Show(string text, int? intend, ConsoleColor color = ConsoleColor.White)
        {
            ConsoleColor origin = Console.ForegroundColor;
            Console.ForegroundColor = color;

            StringBuilder intendStr = new StringBuilder(string.Empty);
            if (intend != null)
                intendStr.Append(' ', intend.Value);

            Console.Write(intendStr + text);
            Console.ForegroundColor = origin;
        }

        public static void ShowIfNotNullOrEmpty(string text, ConsoleColor color = ConsoleColor.White)
        {
            if (!string.IsNullOrEmpty(text)) Show(text, color);
        }

        public static void ShowCutLine(char cutSymbol = '-', ConsoleColor color = ConsoleColor.Green)
        {
            for (int i = 0; i < Console.WindowWidth; i++)
                Show(cutSymbol.ToString(), color);
        }

        public static void PressAnyKeyToContinue(string msg = null, ConsoleColor msgColor = ConsoleColor.White)
        {
            if (!string.IsNullOrEmpty(msg)) ShowLine(msg, msgColor);
            ShowLine("按任意键继续...", ConsoleColor.Green);
            Console.ReadKey();
        }

        public static void ShowSuccessMsg()
        {
            Console.WriteLine();
            ShowLine("  操作成功!\r\n", HighLightColor);
        }

        public static void ShowError(Exception ex = null)
        {
            Console.WriteLine();
            ShowLine($"操作失败!\r\n  {ex?.Message}", ErrorColor);
        }
    }
}
