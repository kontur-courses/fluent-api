using System;

namespace ObjectPrinting.Configs.Children;

public interface IChildrenConfig<TOwner, out TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
}