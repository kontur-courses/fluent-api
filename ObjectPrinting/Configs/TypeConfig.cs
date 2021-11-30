using System;

namespace ObjectPrinting.Configs
{
    public class TypeConfig<TOwner, TType> : INestedPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly Type type;

        public TypeConfig(PrintingConfig<TOwner> config)
        {
            type = typeof(TType);
            this.config = config;
        }

        public PrintingConfig<TOwner> With(Func<TType, string> serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            config.AddTypeSerializer(type, serializer);
            return config;
        }
    }
}