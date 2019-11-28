using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Type type;
        private readonly PropertyInfo property;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.PrintingConfig => printingConfig;
        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property => property;
        Type IPropertyPrintingConfig<TOwner, TPropType>.Type => type;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Type type)
        {
            this.printingConfig = printingConfig;
            this.type = type;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property)
        {
            this.printingConfig = printingConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> func)
        {
            printingConfig.TypePrintingConfig[typeof(TPropType)] = func;
            return printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            (printingConfig as IPrintingConfig<TOwner>).Cultures.Add(typeof(TPropType), culture);
            return printingConfig;
        }
    }
}