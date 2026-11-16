using System;

namespace ObjectPrinting.Configuration;

public interface IStringPropertyConfig<TOwner>
{
    PrintingConfig<TOwner> Exclude();
    PrintingConfig<TOwner> Using(Func<object, string> serializer);
    PrintingConfig<TOwner> TrimToLength(int maxLength);
}