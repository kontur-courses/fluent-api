using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class MemberPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TMemType>(
            this MemberPrintingConfig<TOwner, TMemType> memConfig,
            CultureInfo culture
        )
            where TMemType : IFormattable
        {
            string Serializer(TMemType obj) => obj.ToString(null, culture);
            return memConfig.Using(Serializer);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this MemberPrintingConfig<TOwner, string> memConfig,
            int maxLen
        )
        {
            string Serializer(string obj) => obj.Substring(0, Math.Min(obj.Length, maxLen));
            memConfig.Using(Serializer);
            return ((IMemberPrintingConfig<TOwner, string>) memConfig).ParentConfig;
        }
    }
}