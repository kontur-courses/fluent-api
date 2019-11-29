using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core.PropertyPrintingConfig
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}