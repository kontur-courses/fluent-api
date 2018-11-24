using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private Func<TPropType, string> printingFunction;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingFunction)
        {
            this.printingFunction = printingFunction;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.PrintingConfig => printingConfig;
        Func<TPropType, string> IPropertyPrintingConfig<TOwner, TPropType>.PrintingFunction => printingFunction;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        Func<TPropType, string> PrintingFunction { get; }
    }
}
