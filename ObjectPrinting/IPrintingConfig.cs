namespace ObjectPrinting.Solved
{
    public interface IPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}