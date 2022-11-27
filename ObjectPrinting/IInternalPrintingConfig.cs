namespace ObjectPrinting;

internal interface IInternalPrintingConfig<TOwner>
{
    RootPrintingConfig<TOwner> GetRoot();
    
    PrintingConfig<TOwner>? ParentConfig { get; }
}