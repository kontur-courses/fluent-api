using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private PrintingConfig<TOwner> config;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config) =>
            this.config = config;
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> printer)
        {
            return config;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.ParentConfig => config;

        public PrintingConfig<TOwner> Excluding(Func<TOwner, TPropType> printer)
        {
            return config;
        }
    }
}