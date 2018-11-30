using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public PrintingConfig<TOwner> PrintingConfig => printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            printingConfig.AddTypeSerialization<TPropType>(obj => serializationMethod((TPropType)obj));
            return printingConfig;
        }
    }
}