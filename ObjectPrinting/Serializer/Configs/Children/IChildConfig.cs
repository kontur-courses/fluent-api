using System;

namespace ObjectPrinting.Serializer.Configs.Children;

public interface IChildConfig<TOwner, out TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
}