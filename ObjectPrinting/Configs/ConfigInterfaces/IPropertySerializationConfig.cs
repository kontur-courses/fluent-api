using System.Reflection;

namespace ObjectPrinting.Configs.ConfigInterfaces
{
    public interface IPropertySerializationConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo PropertyInfo { get; }
    }
}