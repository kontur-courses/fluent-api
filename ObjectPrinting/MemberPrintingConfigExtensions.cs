using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, T>(
            this IMemberPrintingConfig<TOwner, T> config,
            CultureInfo cultureInfo, string format = null) where T : IFormattable
        {
            return config.Using(prop => prop.ToString(format, cultureInfo));
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this IMemberPrintingConfig<TOwner, string> config, 
            int trimLength)
        {
            if (trimLength < 0)
                throw new ArgumentException($"Trim length should be positive, but was {trimLength}");
            return config.Using(s => s[..Math.Min(trimLength, s.Length)]);
        }
    }
}
