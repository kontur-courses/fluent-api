using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class ConcretePropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>, IConcretePropertyPrintingConfig
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;

        public ConcretePropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            ((IPrintingConfig) printingConfig).WaysToSerializeProperties[propertyInfo] = obj => print((TPropType) obj);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        PropertyInfo IConcretePropertyPrintingConfig.PropertyInfo => propertyInfo;
    }
}