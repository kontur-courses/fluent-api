using System;

namespace ObjectPrinting.Modules.PrintingConfig
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName = null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyName == null)
                printingConfig.SetTypeSerialization(print);
            else
                printingConfig.SetPropertySerialization(propertyName, print);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        string IPropertyPrintingConfig<TOwner, TPropType>.PropertyName => propertyName;
    }
}