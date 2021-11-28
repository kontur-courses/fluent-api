using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingConfig.UpdatePrintType(typeof(TPropType), print);
            return printingConfig;
        }
    }
}