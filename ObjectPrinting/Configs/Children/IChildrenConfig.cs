namespace ObjectPrinting.Configs.Children;

public interface IChildrenConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}