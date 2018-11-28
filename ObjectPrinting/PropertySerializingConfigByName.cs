using System;

namespace ObjectPrinting
{
    public class PropertySerializingConfigByName<TOwner, TPropertyType>
    {
        private PrintingConfig<TOwner> PrintingConfig { get; }
        private string PropertyName { get; }

        public PropertySerializingConfigByName(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            PrintingConfig = printingConfig;
            PropertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) PrintingConfig).AddNameSerializer(PropertyName, serializer);

            return PrintingConfig;
        }
    }
}
