using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }

        PropertyInfo PropertyInfo { get; }
    }
}