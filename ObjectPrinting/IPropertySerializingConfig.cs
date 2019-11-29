using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertySerializingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string CurrentName { get; }
    }
}