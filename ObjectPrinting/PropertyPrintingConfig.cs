using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo = null)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var config = (IPrintingConfig<TOwner>)printingConfig;
            if (propertyInfo == null)
                config.TypeSerialization[typeof(TPropType)] = print;
            else
                config.PropertySerialization[propertyInfo] = print;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.PropertyInfo => propertyInfo;
    }
}
