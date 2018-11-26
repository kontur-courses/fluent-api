using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TPropType> : ITypePrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            return PrintingConfig;
        }      
    }
}