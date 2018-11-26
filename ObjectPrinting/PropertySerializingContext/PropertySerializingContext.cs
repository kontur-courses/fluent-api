using System;

namespace ObjectPrinting
{
    public class PropertySerializingContext<TOwner, TType> : IPropertySerializingContext<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly (Type, string) property;

        public PropertySerializingContext(PrintingConfig<TOwner> config, (Type, string) property)
        {
            this.config = config;
            this.property = property;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializingMethod)
        {
            ((IPrintingConfig<TOwner>)config).PropertySerializationSettings.Add(property, o => serializingMethod((TType)o));
            return config;
        }

        PrintingConfig<TOwner> IPropertySerializingContext<TOwner>.Config => config;
        (Type, string) IPropertySerializingContext<TOwner>.Property => property;
    }
}