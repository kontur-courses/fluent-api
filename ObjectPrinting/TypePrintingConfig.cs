using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : IInnerPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IInnerPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializator)
        {
            printingConfig.AddAlternativeTypeSerializator(typeof(TPropType), serializator);
            return printingConfig;
        }       
    }
}