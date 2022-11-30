using System;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, TType> : IChildPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> print)
        {
            printingConfig.SetSerializer(typeof(TType), o => print((TType)o));
            return printingConfig;
        }

        PrintingConfig<TOwner> IChildPrintingConfig<TOwner, TType>.ParentConfig => printingConfig;
    }
}