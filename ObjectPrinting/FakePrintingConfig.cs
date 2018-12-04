using System;

namespace ObjectPrinting
{
    public class FakePrintingConfig<TOwner, TPropType> : IMemberPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public FakePrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> format)
        {
            return printingConfig;
        }
    }
}