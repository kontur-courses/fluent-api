using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.PropertyInfo => propertyInfo;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PrintingConfig<TOwner> Using<IFormatable>(CultureInfo cultureInfo)
        {
            ((IPrintingConfig<TOwner>)printingConfig).SerializationSettings.AddCultureForType(typeof(TPropType),
                cultureInfo);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printProperty)
        {
            if (propertyInfo == null)
                ((IPrintingConfig<TOwner>)printingConfig).SerializationSettings.AddTypeSerialization(printProperty);
            else
                ((IPrintingConfig<TOwner>)printingConfig).SerializationSettings.AddPropertySerialization(propertyInfo,
                    printProperty);

            return printingConfig;
        }
    }
}