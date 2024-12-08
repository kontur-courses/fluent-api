using System;

namespace ObjectPrinting;

public interface ITypeSerializer<TParam, TOwner>
{
    public PrintingConfig<TOwner> Use(Func<TParam, string?> serializer);
}