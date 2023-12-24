using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var culturedFunc = new Func<string, string>(s => s[..Math.Min(maxLen, s.Length)]);
            var castedFunc = new Func<object, string>(obj => culturedFunc((string)obj));
            var parent = propConfig.PrintingConfig;
            parent.propertySerializers.Add(propConfig.PropertyInfo, castedFunc);
            return parent;
        }

    }
}