using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IPrintingConfigContainer<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            ((IPrintingConfig<TOwner>) printingConfig).SetSerializerFor(serializer);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPrintingConfigContainer<TOwner>.PrintingConfig => printingConfig;
    }
}
