using ObjectPrinting.Solved;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public static class MemberConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config, int nestingLevel)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj, nestingLevel);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this MemberConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.PrintAs(x => x[..^maxLen]);
            return propConfig.ParentConfig;
        }
    }
}
