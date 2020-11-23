using System;
using System.Globalization;
using ObjectPrinting.Contracts;

namespace ObjectPrinting.Extensions
{
    public static class ContextPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this IContextPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture)
            where TPropType : IFormattable
        {
            return propConfig.Using(d => Convert.ToString(d, culture));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IContextPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(s => s.Length >= maxLen ? s.Substring(0, maxLen) : s);
        }
    }
}