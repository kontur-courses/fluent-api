using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo ConfiguredProperty { get; }
    }
}
