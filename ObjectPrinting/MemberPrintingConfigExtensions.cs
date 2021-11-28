using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, T>(
            this IMemberPrintingConfig<TOwner, T> config,
            CultureInfo cultureInfo) where T : IFormattable
        {
            return config.Using(prop => prop.ToString(null, cultureInfo));
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this IMemberPrintingConfig<TOwner, string> config, 
            int trimLength)
        {
            return config.Using(s => s[..Math.Min(trimLength, s.Length)]);
        }
    }
}
