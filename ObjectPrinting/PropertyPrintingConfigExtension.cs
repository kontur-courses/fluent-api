using System;
using ObjectPrinting.Solved;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtension
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
        
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig
                .TrimForStringProperties[propConfig.MemberSelector] = maxLen;
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}