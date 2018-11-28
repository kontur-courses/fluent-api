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

        public static PrintingConfig<TOwner> TrimmingToLength<TOwner>(
            this PropertySerializingConfigByType<TOwner, string> propertySerializingConfigByType, int maxLength)
        {
            ((IPrintingConfig) propertySerializingConfigByType.PrintingConfig)
                .AddTrimmingSerializer<string>(property => property.Substring(0, Math.Min(maxLength, property.Length)));

            return propertySerializingConfigByType.PrintingConfig;
        }
    }
}
