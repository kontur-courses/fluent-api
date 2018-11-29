using System;
using System.Collections.Generic;

namespace ObjectPrinting.SerializingConfig
{
    public class SerializingConfig<TOwner, TPropertyType> : ISerializingConfig<TOwner, TPropertyType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Func<object, string>> typeOperations;

        public SerializingConfig(PrintingConfig<TOwner> printingConfig, Dictionary<Type, Func<object, string>> typeOperations)
        {
            this.printingConfig = printingConfig;
            this.typeOperations = typeOperations;
        }

        public PrintingConfig<TOwner> Using(Func<object, string> operation)
        {
            typeOperations[typeof(TPropertyType)] = operation;
            return printingConfig;
        }

        PrintingConfig<TOwner> ISerializingConfig<TOwner, TPropertyType>.SerializingConfig => printingConfig;
        Dictionary<Type, Func<object, string>> ISerializingConfig<TOwner, TPropertyType>.TypeOperations => typeOperations;
    }
}