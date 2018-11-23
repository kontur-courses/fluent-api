using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private Func<object, string> printingFunction;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => printingConfig;

        Func<object, string> IPropertyPrintingConfig<TOwner>.PrintingFunction
        {
            get => printingFunction;
            set => printingFunction = value;
        }

        CultureInfo IPropertyPrintingConfig<TOwner>.Culture { get; set; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingFunction = property => print((TPropType) property);
            return printingConfig;
        }
    }
}