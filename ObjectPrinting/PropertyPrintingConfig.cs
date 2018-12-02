using System;
using System.Globalization;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
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

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printWay)
        {   
            if (propertyName is null)
                ((IPrintingConfig<TOwner>) printingConfig).AlternativeTypePrinting[typeof(TPropType)] = printWay;
            else
                ((IPrintingConfig<TOwner>) printingConfig).AlternativePropertyPrinting[propertyName] = printWay;

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig<TOwner>)printingConfig).CulturesForPrinting[typeof(TPropType)] = culture;
            return printingConfig;
        }
        
    }

}