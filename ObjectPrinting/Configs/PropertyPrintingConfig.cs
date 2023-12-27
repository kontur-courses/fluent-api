using System;
using System.Reflection;

namespace ObjectPrinting.Configs
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        private PropertyInfo property;
        // private Func<object, string> serializer;
        
        public PrintingConfig<TOwner> PrintingConfig { get; }
        public Func<object, string> Serializer { get; private set; }
        // public Func<object, string> IPropertyPrintingConfig<TOwner>.Serializer { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property)
        {
            PrintingConfig = printingConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            Serializer = obj =>
            {
                if (obj is TPropType value)
                    return serializer(value);

                throw new ArgumentException();
            };

            return PrintingConfig;
        }
    }
}