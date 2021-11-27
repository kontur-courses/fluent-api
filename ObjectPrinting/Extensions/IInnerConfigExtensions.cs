using System;
using System.Globalization;

namespace ObjectPrinting.Extensions
{
    public static class IInnerConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this IInnerPrintingConfig<TOwner, TPropType> config, CultureInfo cultureInfo) where TPropType : IFormattable
        {
            return config.Using(obj => obj.ToString(null, cultureInfo));
        }

        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this IInnerPrintingConfig<TOwner, TPropType> config, string format, CultureInfo cultureInfo = null) where TPropType : IFormattable
        {
            return config.Using(obj => obj.ToString(format, cultureInfo));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this IInnerPrintingConfig<TOwner, string> config, int maxLen)
        {
            if (maxLen < 0)
                throw new ArgumentException("Length cannot be negative");

            return config.Using(str => str[..Math.Min(str.Length, maxLen)]);
        }
    }
}