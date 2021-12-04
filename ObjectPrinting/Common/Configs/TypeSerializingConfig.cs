using System;
using System.Collections.Generic;

namespace ObjectPrinting.Common.Configs
{
    public class TypeSerializingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> serializers;

        public TypeSerializingConfig(PrintingConfig<TOwner> printingConfig,
            Dictionary<Type, Func<object, string>> serializers)
        {
            this.printingConfig = printingConfig;
            this.serializers = serializers;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializer)
        {
            serializers[typeof(TPropType)] = x => serializer((TPropType) x);
            return printingConfig;
        }
    }
}