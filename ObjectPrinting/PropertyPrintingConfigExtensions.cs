using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj,
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
        {
            return config(ObjectPrinter.For<TOwner>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> printingConfig, CultureInfo cultureInfo)
            where TPropType : IFormattable
        {
            return printingConfig.Using(numericValue => string.Format(cultureInfo, "{0}", numericValue));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(s => s.Substring(0, maxLen));
        }
    }
}