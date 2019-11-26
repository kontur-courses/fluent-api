using System;
using System.Globalization;
using System.Reflection;
using ObjectPrintingHomeTask.Config;

namespace ObjectPrintingHomeTask.PropertyConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property = null)
        {
            this.printingConfig = printingConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (property != null)
                ((IPrintingConfig)printingConfig)?.AddChangedProperty(property, print);
            else
                ((IPrintingConfig)printingConfig)?.AddChangedType(typeof(TPropType), print);
            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            ((IPrintingConfig)printingConfig)?.ChangeCulture(culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}
