namespace ObjectPrinting;

public interface IInternalPrintingConfig<TOwner>
{
    PrintingConfig<TOwner>? ParentConfig { get; }
    RootPrintingConfig<TOwner> GetRoot();
}