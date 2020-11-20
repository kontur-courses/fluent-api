using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtentions
    {
        public static PrintingConfig<TOwner> Using<TOwner, TPropType>(
            this PropertyPrintingConfig<TOwner, TPropType> propertyPrintingConfig,
            CultureInfo cultureInfo) where TPropType : IFormattable
        {
            var config = (IPropertyPrintingConfig<TOwner, TPropType>)propertyPrintingConfig;
            var parentConfig = (IPrintingConfig<TOwner>)config.ParentConfig;
            Func<TPropType, string> serializer = obj => obj.ToString(null, cultureInfo);
            parentConfig.TypeSerialization[typeof(TPropType)] = serializer;
            return config.ParentConfig;
        }

        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int maxLen)
        {
            var propertyConfig = (IPropertyPrintingConfig<TOwner, string>)propertyPrintingConfig;
            var parentConfig = (IPrintingConfig<TOwner>)propertyConfig.ParentConfig;
            var propertyInfo = propertyConfig.PropertyInfo;
            Func<string, string> serializer = str => str.Substring(0, Math.Min(str.Length, maxLen));
            parentConfig.PropertySerialization[propertyInfo] = serializer;
            return propertyConfig.ParentConfig;
        }
    }
}
