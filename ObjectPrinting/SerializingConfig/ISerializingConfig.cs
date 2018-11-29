using System;
using System.Collections.Generic;

namespace ObjectPrinting.SerializingConfig
{
    public interface ISerializingConfig<TOwner, TPropertyType>
    {
        PrintingConfig<TOwner> SerializingConfig { get; }
        Dictionary<Type, Func<object, string>> TypeOperations { get; }
    }
}