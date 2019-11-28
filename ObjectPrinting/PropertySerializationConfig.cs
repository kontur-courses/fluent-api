using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializationConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propInfo;

        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propInfo)
        {
            this.parentConfig = parentConfig;
            this.propInfo = propInfo;
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;
        PropertyInfo IPropertySerializingConfig<TOwner>.PropInfo => propInfo;


        public PrintingConfig<TOwner> WithSerialization(Func<TPropType, string> serializationFunc)
        {
            string Serialization(object property)
            {
                return serializationFunc((TPropType) property);
            }

            if (propInfo == null)
                (parentConfig as IPrintingConfig).SerializationInfo.AddTypeRule(typeof(TPropType),
                    Serialization);
            else
                (parentConfig as IPrintingConfig).SerializationInfo.AddPropertyRule(propInfo,
                    Serialization);
            return parentConfig;
        }
    }
}