using System;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, T> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;
        public PropertyPrintingConfig(PrintingConfig<TOwner> owner)
        {
            config = owner;
        }

        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            throw new NotImplementedException();
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;
    }
}