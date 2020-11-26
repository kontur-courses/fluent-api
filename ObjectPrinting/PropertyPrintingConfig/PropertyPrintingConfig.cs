using System;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public string PropertyName { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            ParentConfig = printingConfig;
            PropertyName = propertyName;
        }

        public PrintingConfig<TOwner> WithConfig(Func<TPropType, string> config)
        {
            var parentConfig = (IPrintingConfig<TOwner>) ParentConfig;
            string PrintingMethod(object obj) => config((TPropType) obj);
            var propertyType = typeof(TPropType);

            if (PropertyName == null)
                parentConfig.TypesPrintingMethods[propertyType] = PrintingMethod;
            else
                parentConfig.PropertiesPrintingMethods[PropertyName] = PrintingMethod;

            return ParentConfig;
        }
    }
}