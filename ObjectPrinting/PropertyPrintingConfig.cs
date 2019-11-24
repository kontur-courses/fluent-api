using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => this.parentConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializationFunc)
        {
            return (parentConfig as IPrintingConfig<TOwner>).AddCustomPrint(serializationFunc);
        }
    }
}
