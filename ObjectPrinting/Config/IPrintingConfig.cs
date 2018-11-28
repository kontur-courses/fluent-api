namespace ObjectPrinting.Config
{
    public interface IPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}