using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName) : this(printingConfig)
        {
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyName is null)
                ((IPrintingConfig) printingConfig).TypePrinting[typeof(TPropType)] = print;
            else
                ((IPrintingConfig) printingConfig).MemberPrinting[propertyName] = print;

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig) printingConfig).CulturesForPrinting[typeof(TPropType)] = culture;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.PrintingConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }
}