using System;

namespace ObjectPrinting
{
    public static class MemberInfoPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this MemberInfoPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            var config = ((IMemberInfoPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            config.AddCutMemberInfo(propConfig.MemberInfoName, maxLen);
            return config;
        }
    }
}