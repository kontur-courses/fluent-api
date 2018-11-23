using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        PropertyInfo Property { get; }
    }
}