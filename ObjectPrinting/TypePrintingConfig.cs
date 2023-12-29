using System;
using System.Collections.Generic;

namespace ObjectPrinting
{
    public class TypePrintingConfig<TOwner>
    {
        private Type withType;
        private PrintingConfig<TOwner> context;

        public readonly Dictionary<Type, Func<object, string>> TypeSerialize
            = new Dictionary<Type, Func<object, string>>();

        public PrintingConfig<TOwner> SpecificSerialization(Func<object, string> serializer)
        {
            TypeSerialize.Add(withType, serializer);

            return context;
        }
        
        public TypePrintingConfig<TOwner> SwapContext<T>(PrintingConfig<TOwner> printingConfig)
        {
            withType = typeof(T);
            context = printingConfig;

            return this;
        }
    }
}