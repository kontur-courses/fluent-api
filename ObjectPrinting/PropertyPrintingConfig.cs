using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProp> : IPropertyPrintingConfig<TOwner, TProp>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> serialize)
        {
            return parentConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TProp>.ParentConfig => parentConfig;
    }
}