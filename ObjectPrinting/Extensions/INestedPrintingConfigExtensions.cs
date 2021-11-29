using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class INestedPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner,TMember>(this INestedPrintingConfig<TOwner, 
            TMember> config, CultureInfo culture, string format = null)
        where TMember: IFormattable
        {
            return config.Using(item => item.ToString(format, culture));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this INestedPrintingConfig<TOwner,
            string> config, int length)
        {
            if (length < 0) throw new ArgumentException($"{nameof(length)} is negative, but should be not", nameof(length));
            return config.Using(str => str[..Math.Min(length, str.Length)]);
        }
    }
}