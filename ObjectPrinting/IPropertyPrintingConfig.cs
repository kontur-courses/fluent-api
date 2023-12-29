namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TMemberType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}