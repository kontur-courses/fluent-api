using System;

namespace ObjectPrinting
{
    public class PropertySerializationConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private readonly string currentName;
        private readonly PrintingConfig<TOwner> parentConfig;

        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        public PropertySerializationConfig(PrintingConfig<TOwner> parentConfig, string currentName)
        {
            this.parentConfig = parentConfig;
            this.currentName = currentName;
        }

        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;
        string IPropertySerializingConfig<TOwner>.CurrentName => currentName;


        public PrintingConfig<TOwner> WithSerialization(Func<TPropType, string> serializationFunc)
        {
            string Serialization(object property)
            {
                return serializationFunc((TPropType) property);
            }

            if (string.IsNullOrEmpty(currentName))
                (parentConfig as IPrintingConfig).SerializationInfo.AddTypeSerializationRule(typeof(TPropType),
                    Serialization);
            else
                (parentConfig as IPrintingConfig).SerializationInfo.AddSerializationRule(currentName,
                    Serialization);
            return parentConfig;
        }
    }
}