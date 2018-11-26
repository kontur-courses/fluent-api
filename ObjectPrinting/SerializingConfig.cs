using System;

namespace ObjectPrinting
{
    public class SerializingConfig<TOwner, TPropertyType> : ISerializingConfig<TOwner, TPropertyType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public SerializingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropertyType, string> operation)
        {
            printingConfig.SetTypeOperation(typeof(TPropertyType), operation);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> ISerializingConfig<TOwner, TPropertyType>.SerializingConfig => printingConfig;
    }
}