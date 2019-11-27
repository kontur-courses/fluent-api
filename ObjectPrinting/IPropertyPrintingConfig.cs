using System.Reflection;

namespace ObjectPrinting
{
    interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo PropertyInfo { get; }
    }
}
