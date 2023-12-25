namespace ObjectPrinting;

public interface ITypePrintingConfig<TOwner, TType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}
