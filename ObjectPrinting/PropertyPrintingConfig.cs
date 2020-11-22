using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Action<Func<TPropType, string>> printingFunction;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Action<Func<TPropType, string>> func)
        {
            this.printingConfig = printingConfig;
            printingFunction = func;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            printingFunction(print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.AddCultureForType<TPropType>(culture);
            return printingConfig;
        }
    }
}