using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config, int maxNestingLevel)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj, maxNestingLevel);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            Func<object, string> trimmingFunc = str => ((string)str).Substring(0, maxLen);
            return propConfig.Using(trimmingFunc);
        }

        public static PrintingConfig<TOwner> UsingCulture<TOwner, TPropType>(this TypePrintingConfig<TOwner, TPropType> propConfig,
            CultureInfo cultureInfo, string format = null)
            where TPropType : IFormattable
        {
            Func<TPropType, string> changeCultureFunc = obj => obj.ToString(format, cultureInfo);
            return propConfig.Using(changeCultureFunc);
        }
    }
}