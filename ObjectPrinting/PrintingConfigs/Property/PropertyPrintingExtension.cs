using System;
using System.Globalization;
using ObjectPrinting.PrintingConfigs;
using ObjectPrinting.Serializer;

namespace ObjectPrinting.PrintingConfigs
{
    public static class PropertyPrintingExtension
    {
        public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> currentConfig, int length)
        {
            var propConfig = currentConfig as IPropertyPrintingConfig<TOwner>;

            (propConfig.Config as IPrintingConfig)
                .ApplyNewSerializationRule(new PropertySerializationRule(propConfig.Filter, 
                (obj, propertyInfo) => 
                    (propertyInfo.GetValue(obj) as string)?.Substring(0, length)));

            return propConfig.Config;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner, T>(this PropertyPrintingConfig<TOwner, T> currentConfig,
            CultureInfo info) where T : IFormattable
        {
            var propConfig = currentConfig as IPropertyPrintingConfig<TOwner>;
            
            (propConfig.Config as IPrintingConfig)
                .ApplyNewSerializationRule(new PropertySerializationRule(propConfig.Filter,
                    (obj, propertyInfo) =>
                        ((T) propertyInfo.GetValue(obj)).ToString(null, info)));

            return propConfig.Config;
        }
    }
}