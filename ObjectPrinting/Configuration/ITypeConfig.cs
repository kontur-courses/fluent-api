using System;

namespace ObjectPrinting.Configuration;

public interface ITypeConfig<TOwner>
{
    PrintingConfig<TOwner> Exclude();
    PrintingConfig<TOwner> Using(Func<object, string> serializer);
}