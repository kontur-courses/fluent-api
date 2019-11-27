using System;

namespace ObjectPrinting.Configs
{
    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Func<object, string> Serializer { get; }
    }
}