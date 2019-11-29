using System;
using System.Globalization;
using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core.PropertyPrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printingFunction)
        {
            if (propertyName == null)
            {
                ((IPrintingConfig) printingConfig).TypePrintingFunctions[typeof(TPropType)] = printingFunction;
            }
            else
            {
                ((IPrintingConfig) printingConfig).PropertyPrintingFunctions[propertyName] =
                    printingFunction;
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig) printingConfig).TypeCultures[typeof(TPropType)] = culture;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => printingConfig;
    }
}