using System;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig,
            int maxLen)
        {
            var serializationConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig as
                ISerializationConfig<TOwner>;
            var memberInfo = ((IPropertyPrintingConfig<TOwner, string>)propConfig).MemberInfo;
            serializationConfig.SetTrim(memberInfo, maxLen);
            return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
        }
    }
}
