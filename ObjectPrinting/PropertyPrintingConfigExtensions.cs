using System;
using System.Collections;
using ObjectPrinting.Solved;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config) =>
            config(ObjectPrinter.For<T>()).PrintToString(obj);


        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen) =>
            propConfig.Using(x => x.Substring(0, maxLen));
    }
}