using ObjectPrinting.Core.PrintingConfig;

namespace ObjectPrinting.Core.PropertyPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}