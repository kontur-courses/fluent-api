using System;

namespace ObjectPrinting
{
    internal class TypeConfig<TOwner, TType> : IInnerTypeConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly SerializerSettings serializerSettings;

        public TypeConfig(PrintingConfig<TOwner> printingConfig, SerializerSettings serializerSettings)
        {
            this.printingConfig = printingConfig;
            this.serializerSettings = serializerSettings;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializer)
        {
            serializerSettings.AddTypeSerializer(serializer);
            return printingConfig;
        }
    }
}