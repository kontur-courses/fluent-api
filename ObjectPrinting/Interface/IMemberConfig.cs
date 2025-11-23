using System;

namespace ObjectPrinting.Interface;

public interface IMemberConfig<TOwner, TProp>
{
    PrintingConfig<TOwner> Using(Func<TProp, string> serializer);
}