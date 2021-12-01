using System;

namespace ObjectPrinting
{
    public class TypeConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        public TypeConfig(PrintingConfig<TOwner> parentConfig)
        {
            printingConfig = parentConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> serializer)
        {
            printingConfig.AddAlternativeTypeSerializer(typeof(TProperty), serializer);
            return printingConfig;
        }
    }
}