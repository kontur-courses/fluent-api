using System;
using System.Globalization;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TType>
            (this MemberPrintingConfig<TOwner, TType> config, CultureInfo culture)
            where TType : IFormattable
        {
            return config.Using(formattable =>
                formattable.ToString(null, culture));
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>
            (this MemberPrintingConfig<TOwner, string> config, int length)
        {
            return config.Using(str =>
            {
                var minLen = Math.Min(str.Length, length);
                return str.Substring(0, minLen);
            });
        }
    }
}