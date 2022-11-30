using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        private readonly PropertyInfo propertyInfo;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printOption)
        {
            if (propertyInfo != null)
                printingConfig.AddPropertySerializationOption(propertyInfo, printOption);
            else
                printingConfig.AddTypeSerializationOption(typeof(TPropType), printOption);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            printingConfig.AddTypeCulture(typeof(TPropType), culture);

            return printingConfig;
        }
    }
}
