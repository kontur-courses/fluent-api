using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner, T>
    {
        private readonly PrintingConfig<TOwner> context;

        private readonly Dictionary<Type, Func<object, string>> typesSerialize;

        public TypePrintingConfig(Dictionary<Type, Func<object, string>> typesSerialize,
            PrintingConfig<TOwner> printingConfig)
        {
            this.typesSerialize = typesSerialize;
            context = printingConfig;
        }

        public PrintingConfig<TOwner> SpecificSerialization(Func<object, string> serializer)
        {
            typesSerialize.Add(typeof(T), serializer);
            return context;
        }
    }
}