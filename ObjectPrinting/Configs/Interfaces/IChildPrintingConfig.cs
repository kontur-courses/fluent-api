namespace ObjectPrinting.Configs.Interfaces;

public interface IChildPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}