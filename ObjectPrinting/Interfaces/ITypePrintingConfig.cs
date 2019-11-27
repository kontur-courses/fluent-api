namespace ObjectPrinting
{
    interface ITypePrintingConfig<TOwner, TType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
