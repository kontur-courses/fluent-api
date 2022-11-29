namespace ObjectPrinting
{
    public interface IChildPrintingConfig<TOwner, TType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}