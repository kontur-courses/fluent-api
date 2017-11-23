namespace ObjectPrinting
{
    public interface IChildPrintingConfig<TOwner, TContent>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}