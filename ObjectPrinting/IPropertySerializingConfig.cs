using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo PropInfo { get; }
    }
}