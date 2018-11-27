using System;
using System.Collections.Generic;

namespace ObjectPrinting.SerializingConfig
{
    public class SerializingConfig<TOwner, TPropertyType> : ISerializingConfig<TOwner, TPropertyType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Dictionary<Type, Delegate> typeOperations;

        public SerializingConfig(PrintingConfig<TOwner> printingConfig, Dictionary<Type, Delegate> typeOperations)
        {
            this.printingConfig = printingConfig;
            this.typeOperations = typeOperations;
        }

        public PrintingConfig<TOwner> Using(Func<TPropertyType, string> operation)
        {
            typeOperations[typeof(TPropertyType)] = operation;
            return printingConfig;
        }



        PrintingConfig<TOwner> ISerializingConfig<TOwner, TPropertyType>.SerializingConfig => printingConfig;
        Dictionary<Type, Delegate> ISerializingConfig<TOwner, TPropertyType>.TypeOperations => typeOperations;
    }
}