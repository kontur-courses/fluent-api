using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string GetStringValueOrDefault(this object obj)
        {
            return obj == null ? "null" : obj.ToString();
        }

        public static string GetStringValueOrDefault(this object obj, CultureInfo cultureInfo)
        {
            return Convert.ToString(obj, cultureInfo);
        }
    }
}