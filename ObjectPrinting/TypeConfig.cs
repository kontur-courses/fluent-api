using System;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly Type configuratedType;

        public TypeConfig(PrintingConfig<TOwner> config, Type configuratedType)
        {
            this.config = config;
            this.configuratedType = configuratedType;
        }

        public PrintingConfig<TOwner> SerializeAs(Func<TProperty, string> f)
        {
            config.AddTypeSerialization(f, configuratedType);
            return config;
        }
    }
}