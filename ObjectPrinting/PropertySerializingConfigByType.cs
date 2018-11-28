using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertySerializingConfigByType<TOwner, TPropertyType>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }

        public PropertySerializingConfigByType(PrintingConfig<TOwner> printingConfig)
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
            this PropertySerializingConfigByType<TOwner, double> propertySerializingConfigByType, CultureInfo cultureInfo)
        {
            ((IPrintingConfig) propertySerializingConfigByType.PrintingConfig)
                .AddCulturallySpecificSerializer<double>(property => property.ToString(cultureInfo));

            return propertySerializingConfigByType.PrintingConfig;
        }
    }
}
