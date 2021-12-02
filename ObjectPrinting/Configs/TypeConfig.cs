using System;

namespace ObjectPrinting.Configs
{
    public class TypeConfig<TOwner, TType> : INestedPrintingConfig<TOwner, TType>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly SerializationSettings settings;

        public TypeConfig(PrintingConfig<TOwner> config, SerializationSettings settings)
        {
            this.config = config;
            this.settings = settings;
        }

        public PrintingConfig<TOwner> With(Func<TType, string> serializer)
        {
            settings.SetSerializer(serializer);
            return config;
        }
    }
}