using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo culture);
        PrintingConfig<TOwner> Trim(int maxLen);
    }

    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Use(Func<TPropType, string> func)
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

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Trim(int maxLen)
        {
            var iParentConfig = parentConfig as IPrintingConfig<TOwner>;
            return propertyInfo == null
                ? iParentConfig.Trim(maxLen)
                : iParentConfig.Trim(propertyInfo.Name, maxLen);
        }
    }
}