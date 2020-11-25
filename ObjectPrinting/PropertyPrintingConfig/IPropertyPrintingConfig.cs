using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.PropertyPrintingConfig
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropertyName { get; }
    }
}