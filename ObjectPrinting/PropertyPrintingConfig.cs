using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp> : IPropertyPrintingConfig<TProp>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private Func<TProp, string> printingFunction;


        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> propertyPrinting)
        {
            printingFunction = propertyPrinting;
            return printingConfig;
        }

        Func<TProp, string> IPropertyPrintingConfig<TProp>.PrintingFunction => printingFunction;
    }
}