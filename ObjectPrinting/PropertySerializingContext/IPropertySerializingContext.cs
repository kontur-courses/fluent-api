using System;

namespace ObjectPrinting
{
    public interface IPropertySerializingContext<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
        (Type, string) Property { get; }
    }
}