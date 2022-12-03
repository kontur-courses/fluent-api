namespace ObjectPrinting.Configuration
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentPrinter { get; }
    }
}