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

        public PropertyPrintingConfig(string propertyName, PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyName != null)
                printingConfig.PropertyConverters[propertyName] = obj => print.Invoke((TPropType) obj);
            else
                printingConfig.TypeConverters[typeof(TPropType)] = obj => print.Invoke((TPropType) obj);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.CulturesForProperties[typeof(TPropType)] = culture;
            return printingConfig;
        }
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}