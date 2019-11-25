using ObjectPrintingHomeTask.Config;

namespace ObjectPrintingHomeTask.PropertyConfig
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
