using System;

namespace ObjectPrinting.Configuration;

public interface IPropertyConfig<TOwner>
{
    PrintingConfig<TOwner> Exclude();
    PrintingConfig<TOwner> Using(Func<object, string> serializer);
    PrintingConfig<TOwner> TrimToLength(int maxLength);
}