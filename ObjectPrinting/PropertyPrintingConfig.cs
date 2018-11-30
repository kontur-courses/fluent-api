using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;
        public PrintingConfig<TOwner> PrintingConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            printingConfig.AddPropertySerialization<TPropType>(propertyInfo, obj => serializationMethod((TPropType)obj));
            return printingConfig;
        }
    }
}