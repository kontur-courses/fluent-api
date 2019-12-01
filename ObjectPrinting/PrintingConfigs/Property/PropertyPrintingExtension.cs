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

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, int> currentConfig,
            CultureInfo info) 
        {
            var propConfig = currentConfig as IPropertyPrintingConfig<TOwner>;

            (propConfig.Config as IPrintingConfig)
                .ApplyNewSerializationRule(new PropertySerializationRule(propConfig.Filter,
                    (obj, propertyInfo) =>
                        ((int) propertyInfo.GetValue(obj)).ToString(info)));

            return propConfig.Config;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, double> currentConfig,
            CultureInfo info)
        {
            var propConfig = currentConfig as IPropertyPrintingConfig<TOwner>;

            (propConfig.Config as IPrintingConfig)
                .ApplyNewSerializationRule(new PropertySerializationRule(propConfig.Filter,
                    (obj, propertyInfo) =>
                        ((double) propertyInfo.GetValue(obj)).ToString(info)));

            return propConfig.Config;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, byte> currentConfig,
            CultureInfo info)
        {
            var propConfig = currentConfig as IPropertyPrintingConfig<TOwner>;

            (propConfig.Config as IPrintingConfig)
                .ApplyNewSerializationRule(new PropertySerializationRule(propConfig.Filter,
                    (obj, propertyInfo) =>
                        ((byte) propertyInfo.GetValue(obj)).ToString(info)));

            return propConfig.Config;
        }

        public static PrintingConfig<TOwner> SetCulture<TOwner>(this PropertyPrintingConfig<TOwner, float> currentConfig,
            CultureInfo info)
        {
            var propConfig = currentConfig as IPropertyPrintingConfig<TOwner>;

            (propConfig.Config as IPrintingConfig)
                .ApplyNewSerializationRule(new PropertySerializationRule(propConfig.Filter,
                    (obj, propertyInfo) =>
                        ((byte) propertyInfo.GetValue(obj)).ToString(info)));

            return propConfig.Config;
        }
    }
}