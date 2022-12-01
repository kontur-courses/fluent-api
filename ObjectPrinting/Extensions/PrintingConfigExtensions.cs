using System;
using System.Globalization;
using ObjectPrinting.PrintingConfiguration;

namespace ObjectPrinting.Extensions
{
    public static class PrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static PrintingConfig<TOwner> SetLength<TOwner>(this MemberPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            propConfig.PrintingConfig.SetMaxLength(propConfig.MemberInfo, maxLen);
            return propConfig.PrintingConfig;
        }
        
        public static PrintingConfig<TOwner> UseCulture<TOwner, TType>(this TypePrintingConfig<TOwner, TType> propConfig,
            CultureInfo culture) where TType : IFormattable
        {
            propConfig.PrintingConfig.AddCultureUsing(typeof(TType), culture);
            return propConfig.PrintingConfig;
        }
    }
}