using System;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, T> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;

        public PropertySerializingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public Func<T, string> Serializer = null;
        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            Serializer = func;
            return parentConfig;
        }

        public string Serialize(object property)
        {
            if (property is T typedProperty && Serializer != null)
                return Serializer(typedProperty) + Environment.NewLine;
            else
                throw new ArgumentException();
        }
    }
}
