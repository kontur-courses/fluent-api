using ObjectPrinting.Solved;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public static class MemberConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this MemberConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.PrintAs(x => x.Substring(0, x.Length - maxLen));
            return propConfig.ParentConfig;
        }
    }
}
