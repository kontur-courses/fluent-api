namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TProp>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}