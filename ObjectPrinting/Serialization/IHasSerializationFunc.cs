using System;

namespace ObjectPrinting.Serialization
{
    public interface IHasSerializationFunc
    {
        Func<object, string> SerializationFunc { get; }
    }
}