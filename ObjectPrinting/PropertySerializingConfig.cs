using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, TPropType> :
        IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyInfo;

        public PropertySerializingConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertySerializingConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyInfo)
        {
            this.parentConfig = parentConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializingMethod)
        {
            if (propertyInfo is null)
                (parentConfig as IPrintingConfig<TOwner>).SerializingMethods.Add(typeof(TPropType),
                    type => serializingMethod((TPropType) type));
            else
            {
                (parentConfig as IPrintingConfig<TOwner>).PropertySerializingMethods.Add(propertyInfo,
                    prop => serializingMethod((TPropType) prop));
            }

            return parentConfig;
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertySerializingConfig<TOwner>.UsedPropertyInfo => propertyInfo;
    }
}
