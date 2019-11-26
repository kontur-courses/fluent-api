using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializationConfig<TOwner, TPropType> : IPropertySerializationConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly PropertyInfo propertyToSetup;
        private readonly Type typeToSetup;
        private readonly SerializableObjectType serializableObjectType;

        PrintingConfig<TOwner> IPropertySerializationConfig<TOwner>.ParentConfig => parentConfig;

        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig, PropertyInfo propertyToSetup)
        {
            this.parentConfig = parentConfig;
            this.propertyToSetup = propertyToSetup;
            serializableObjectType = SerializableObjectType.Property;
        }
        
        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig, Type typeToSetup)
        {
            this.parentConfig = parentConfig;
            this.typeToSetup = typeToSetup;
            serializableObjectType = SerializableObjectType.Type;
        }

        public PrintingConfig<TOwner> WithSerialization(Func<TPropType, string> serializationFunc)
        {
            string Wrapper(object property) => serializationFunc((TPropType) property); // Is it okay to use local functions?

            switch (serializableObjectType)
            {
                case SerializableObjectType.Property:
                    return new PrintingConfig<TOwner>(parentConfig).WithCustomPropertyRule(propertyToSetup, Wrapper);
                case SerializableObjectType.Type:
                    return new PrintingConfig<TOwner>(parentConfig).WithCustomTypeRule(typeToSetup, Wrapper);
                default:
                    throw new ArgumentException();
            }
        }
    }
}