using System;
using System.Globalization;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public static class PropertyPrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
        
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(this PropertyPrintingConfig<TOwner, TPropType> propConfig, CultureInfo culture, string format  = null) where TPropType : IFormattable
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, TPropType>)propConfig).ParentConfig;

            Func<TPropType, string> function = obj => obj.ToString(format, culture);

            parentConfig.MemberSerializationRule.AddRule(propConfig.MemberInfo, function);

            return parentConfig;
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