using System;

namespace ObjectPrinting
{
    public interface ISerializerConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Func<object, string> SerializeFunc { get; set; }
    }
}