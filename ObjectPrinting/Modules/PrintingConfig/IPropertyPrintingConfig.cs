namespace ObjectPrinting.Modules.PrintingConfig
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropertyName { get; }
    }
}