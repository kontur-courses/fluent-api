using System;

namespace ObjectPrinting
{
    public interface IHasSerializationFunc
    {
        Func<object, string> SerializationFunc { get; }
    }
}