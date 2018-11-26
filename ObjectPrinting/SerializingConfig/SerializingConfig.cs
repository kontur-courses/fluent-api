using System;

namespace ObjectPrinting.SerializingConfig
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
            printingConfig.typeOperations[typeof(TPropertyType)] = operation;
            return printingConfig;
        }

        PrintingConfig<TOwner> ISerializingConfig<TOwner, TPropertyType>.SerializingConfig => printingConfig;
    }
}