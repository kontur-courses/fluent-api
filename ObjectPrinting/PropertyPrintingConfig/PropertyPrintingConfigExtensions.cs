using System;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLength)
        {
            if (maxLength < 1)
                throw new ArgumentException("maxLength must be greater than 1");

            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;

            Func<string, string> function = str =>
                string.IsNullOrEmpty(str) || str.Length <= maxLength ? 
                str : 
                str.Substring(0, maxLength);

            parentConfig.MemberSerializationRule.AddRule(propConfig.MemberInfo, function);
            return parentConfig;
        }

    }
}