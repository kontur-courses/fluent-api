using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, TPropertyType>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }

        public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) PrintingConfig).AddTypeSerializer(serializer);

            return PrintingConfig;
        }
    }

    public static class PropertySerializingConfigExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(
            this PropertySerializingConfig<TOwner, double> propertySerializingConfig, CultureInfo cultureInfo)
        {
            ((IPrintingConfig) propertySerializingConfig.PrintingConfig)
                .AddCulturallySpecificSerializers<double>(property => property.ToString(cultureInfo));

            return propertySerializingConfig.PrintingConfig;
        }
    }
}
