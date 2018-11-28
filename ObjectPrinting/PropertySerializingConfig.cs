using System;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, TPropertyType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropertyType, string> serializer)
        {
            ((IPrintingConfig) printingConfig).AddTypeSerializer(serializer);

            return printingConfig;
        }
    }
}
