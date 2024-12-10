namespace ObjectPrinting;

public interface IPropertyPrintingConfig<TOwner>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}