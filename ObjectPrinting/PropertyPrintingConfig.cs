using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyName == null)
                ((IPrintingConfig<TOwner>)printingConfig).TypesWithSpecificPrint.Add(typeof(TPropType), print);
            else
                ((IPrintingConfig<TOwner>)printingConfig).NamesWithSpecificPrint.Add(propertyName, print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig<TOwner>)printingConfig).TypesWithCulture.Add(typeof(TPropType), culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        string IPropertyPrintingConfig<TOwner, TPropType>.PropertyNameToTrimValue => propertyName;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropertyNameToTrimValue { get; }
    }
}