using System;

namespace ObjectPrinting
{
    public class TypeSerializationConfig<TOwner, TTarget> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;

        public TypeSerializationConfig(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public PrintingConfig<TOwner> Using(Func<TTarget, string> serializer)
        {
            var childConfig = new PrintingConfig<TOwner>(config);
            childConfig.TypeSerializers.Add(TypeSerializer.Create(serializer));
            return childConfig;
        }

        public PrintingConfig<TOwner> UsingFormat(Func<string, string, string, string> formatter)
        {
            var childConfig = new PrintingConfig<TOwner>(config);
            childConfig.TypeFormatters.Add(typeof(TTarget), formatter);
            return childConfig;
        }
    }
}