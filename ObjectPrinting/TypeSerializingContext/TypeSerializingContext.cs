using System;

namespace ObjectPrinting
{
    public class TypeSerializingContext<TOwner, TType> : ITypeSerializingContext<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;

        public TypeSerializingContext(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> serializingMethod)
        {
            ((IPrintingConfig<TOwner>)config).TypeSerializationSettings.Add(typeof(TType), o => serializingMethod((TType)o));
            return config;
        }

        PrintingConfig<TOwner> ITypeSerializingContext<TOwner>.Config => config;
    }
}