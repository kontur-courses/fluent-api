using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IInnerPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            printingConfig.AddAlternativeTypeSerializer(typeof(TPropType), serializer);
            return printingConfig;
        }
    }
}