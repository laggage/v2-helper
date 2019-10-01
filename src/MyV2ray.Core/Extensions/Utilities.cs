namespace MyV2ray.Core.Extensions
{
    using System;
    using System.Reflection;

    public static class Utilities
    {
        public static void CreateIfNull<T>(ref T d) where T :new()
        {
            d ??= new T();
        }

        public static T ToInstance<T>(this Type type) where T:class
        {
            if (type == null) throw new ArgumentNullException();

            Assembly a = type.Assembly;
            
            return a.CreateInstance(type.FullName) as T;
        }

        /// <summary>
        /// 将以字节为单位的流量数值自动转换为合适的单位流量数值
        /// </summary>
        /// <param name="value">流量, 单位: 字节(B)</param>
        /// <returns></returns>
        public static string ToTrafficString(this long value)
        {
            string[] units = {"B", "KB", "MB", "GB"};
            double v = value;
            byte i = 0;
            while ((v / 1024.0) > 1)
            {
                v /= 1024;
                i++;
            }

            return string.Format("{0:f2}{1}", v, units[i]);
        }
    }
}
