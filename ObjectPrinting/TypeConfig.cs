using System;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner, TMember>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypeConfig(PrintingConfig<TOwner> parentConfig)
        {
            printingConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TMember, string> serializer)
        {
            printingConfig.AddAlternativeTypeSerializer(typeof(TMember), serializer);
            return printingConfig;
        }
    }
}