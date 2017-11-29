using System;
using System.Globalization;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        public string PropertyName { get; private set; }
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            this.printingConfig = printingConfig;
            PropertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> resializeFuction)
        {
            if (string.IsNullOrEmpty(PropertyName))
                printingConfig.AddTypeSerializing(typeof(TPropType), resializeFuction);
            else
                printingConfig.AddPropertySerialization(PropertyName, resializeFuction);
            return printingConfig;
        }

    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}