using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType> : ITypePrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> print)
        {
            ((IPrintingConfig<TOwner>)printingConfig).TypeSerializers[typeof(TType)] = print;
            return printingConfig;
        }

        PrintingConfig<TOwner> ITypePrintingConfig<TOwner, TType>.ParentConfig => printingConfig;
    }
}
