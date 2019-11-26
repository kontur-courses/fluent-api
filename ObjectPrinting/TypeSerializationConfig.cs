using System;

namespace ObjectPrinting
{
    public class TypeSerializationConfig<TOwner, TTarget> : IPropertyPrintingConfig<TOwner>
    {
        protected PrintingConfig<TOwner> config;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;

        public TypeSerializationConfig(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public PrintingConfig<TOwner> Using(Func<TTarget, string> serializer)
        {
            config.TypeSerializators.Add(typeof(TTarget), TypeSerializer.CreateSerializer(typeof(TTarget), serializer));
            return config;
        }
    }
}