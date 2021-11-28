using System;
using ObjectPrinting.Solved.PrintingConfiguration;

namespace ObjectPrinting.Solved.Extensions
{
    public static class MemberPrintingConfigExtensions
    {
        public static string PrintToString<T>(
            this T obj,
            Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            var printingConfig = config(ObjectPrinter.For<T>());
            return printingConfig.PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this MemberPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            return ((IMemberPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}