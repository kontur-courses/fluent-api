using System;
using System.Globalization;
using ObjectPrinting.PrintingConfiguration;

namespace ObjectPrinting.Extensions
{
    public static class IInnerPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this IInnerPrintingConfig<TOwner, TPropType> config, CultureInfo culture) where TPropType : IFormattable
        {
            return config.Using(p => p.ToString(null, culture));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this IInnerPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return propConfig.Using(str => str[..Math.Min(str.Length, maxLen)]);
        }
    }
}