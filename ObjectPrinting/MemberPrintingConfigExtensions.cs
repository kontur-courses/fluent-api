using System;
using System.Globalization;
using System.Reflection;

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
            var iConfig = (IMemberPrintingConfig<TOwner, string>) memConfig;
            string Serializer(string obj) => obj.Substring(0, Math.Min(obj.Length, maxLen));
            iConfig.ParentConfig.AddMemberSerializer(iConfig.MemberInfo, (Func<string, string>) Serializer);
            return ((IMemberPrintingConfig<TOwner, string>) memConfig).ParentConfig;
        }
    }
}