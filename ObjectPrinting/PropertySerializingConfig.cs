using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, TPropType>
        : IPropertySerializingConfig<TOwner>
    {
        private PrintingConfig<TOwner> parentConfig;
        private PropertyInfo propertyInfo = null;

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertySerializingConfig<TOwner>.PropertyInfo => propertyInfo;

        public PropertySerializingConfig(
            PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo = null)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationFunc)
        {
            if (propertyInfo == null)
                (parentConfig as IPrintingConfig<TOwner>).SerializationForType[typeof(TPropType)]
                    = property => serializationFunc((TPropType)property);
            else
                (parentConfig as IPrintingConfig<TOwner>).SerializationForProperty[propertyInfo]
                    = property => serializationFunc((TPropType)property);
            return parentConfig;
        }
    }
}