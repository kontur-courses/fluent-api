namespace ObjectPrinting
{
    internal interface IPrintingConfigContainer<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
    }
}
