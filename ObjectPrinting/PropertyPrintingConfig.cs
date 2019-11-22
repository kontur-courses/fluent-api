using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo culture);
        PrintingConfig<TOwner> TrimmedToLength(int maxLen);
    }

    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        private PropertyInfo propertyInfo = null;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
        {
            var iParentConfig = parentConfig as IPrintingConfig<TOwner>;
            return propertyInfo == null
                ? iParentConfig.AddCustomSerialization(typeof(TPropType), func)
                : iParentConfig.AddCustomPropertySerialization(propertyInfo.Name, func);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.SetCultureFor<T>(CultureInfo culture)
        {
            return (parentConfig as IPrintingConfig<TOwner>).SetTypeCulture(typeof(T), culture);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.TrimmedToLength(int maxLen)
        {
            var iParentConfig = parentConfig as IPrintingConfig<TOwner>;
            return propertyInfo == null
                ? iParentConfig.TrimmedToLength(maxLen)
                : iParentConfig.TrimmedToLength(propertyInfo.Name, maxLen);
        }
    }
}