using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class ConcretePropertyPrintingConfig<TOwner, TPropType> : IConcretePropertyPrintingConfig<TOwner, TPropType>
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
            var settings = ((IPrintingConfig) printingConfig).Settings;
            return new PrintingConfig<TOwner>(
                settings.AddWayToSerializeProperty(propertyInfo, obj => print((TPropType) obj)));
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        PropertyInfo IConcretePropertyPrintingConfig<TOwner, TPropType>.PropertyInfo => propertyInfo;
    }
}