using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.ConfigProperty => propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Use(Func<TPropType, string> serializer)
        {
            var configInterface = printingConfig as IPrintingConfig<TOwner>;

            if (configInterface.PropertySerializers.ContainsKey(propertyInfo))
                configInterface.PropertySerializers[propertyInfo] = serializer;
            else
                configInterface.PropertySerializers.Add(propertyInfo, serializer);

            return printingConfig;
        }
    }
}