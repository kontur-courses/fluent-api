using System;

namespace ObjectPrinting
{
    public class SerializerConfig<TOwner, T> : ISerializerConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private Func<object, string> propertySerializeFunc;

        public SerializerConfig(PrintingConfig<TOwner> parentConfig)
        {
            this.parentConfig = parentConfig;
        }

        PrintingConfig<TOwner> ISerializerConfig<TOwner>.ParentConfig => parentConfig;

        Func<object, string> ISerializerConfig<TOwner>.SerializeFunc
        {
            get => propertySerializeFunc;
            set => propertySerializeFunc = value;
        }

        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            propertySerializeFunc = x => func((T)x);
            return parentConfig;
        }
    }
}