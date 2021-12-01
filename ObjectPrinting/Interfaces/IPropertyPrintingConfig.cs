namespace ObjectPrinting.Interfaces
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
