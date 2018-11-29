using System;
using System.Linq;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static bool IsNumericType(this Type type)
        {
            return new[]
            {
                typeof(sbyte),
                typeof(short),
                typeof(long),
                typeof(int),
                typeof(byte),
                typeof(int),
                typeof(uint),
                typeof(ulong),
                typeof(float),
                typeof(double),
                typeof(decimal)
            }.Contains(type);
        }
    }
}