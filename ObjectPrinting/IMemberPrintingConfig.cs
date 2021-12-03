namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}