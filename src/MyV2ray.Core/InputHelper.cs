namespace MyV2ray.Core
{
    using System;

    public class InputHelper
    {
        public static string GetInput(
            string tips, string advice = null,
            ConsoleColor tipsColor = ConsoleColor.White,
            ConsoleColor adviceColor = ConsoleColor.Gray)
        {
            Displayer.Show(tips,2, tipsColor);
            if (!string.IsNullOrEmpty(advice))
            {
                Displayer.Show("(", tipsColor);
                Displayer.Show(advice, adviceColor);
                Displayer.Show(")", tipsColor);
            }
            Displayer.Show(": ", tipsColor);
            string input = Console.ReadLine();
            Displayer.ShowLine("");
            return input;
        }

        public static int GetNumberInput(
            string tips = "输入索引来选择", string advice = null,
            ConsoleColor tipsColor = ConsoleColor.White,
            ConsoleColor adviceColor = ConsoleColor.Gray)
        {
            return Convert.ToInt32(GetInput(tips, advice, tipsColor, adviceColor));
        }

        /// <summary>
        /// 返回一个用户输入的数字
        /// </summary>
        /// <param name="tips">提示用户输入的文字</param>
        /// <param name="advice">提供输入建议</param>
        /// <param name="inputRange">发生异常时的提示信息</param>
        /// <param name="tipsColor"><param name="tips"/>的颜色</param>
        /// <param name="adviceColor"><param name="advice"/>的颜色</param>
        /// <returns></returns>
        public static int? TryGetNumberInput(
            string tips = "请输入序号来选择", string advice = null,Tuple<int,int> inputRange = null,
            ConsoleColor tipsColor = ConsoleColor.White,
            ConsoleColor adviceColor = ConsoleColor.Gray)
        {
            try
            {
                int i = GetNumberInput(tips, advice, tipsColor, adviceColor);
                if (inputRange != null)
                {
                    int min = Math.Min(inputRange.Item1, inputRange.Item2);
                    int max = Math.Max(inputRange.Item1, inputRange.Item2);
                    if (i < min ||
                        i > max)
                        throw new FormatException($"输入的整数必须在{min} - {max}之间");
                }
                return i;
            }
            catch(Exception ex)
            {
                Displayer.ShowLine(
                    $"输入格式错误!请输入一个整数!{ex.Message}", 2, Displayer.ErrorColor);
                Displayer.PressAnyKeyToContinue();

                return null;
            }

        }
    }
}
