using System.Reflection;

namespace ObjectPrinting
{
    public interface IConcretePropertyPrintingConfig<TOwner> : IPropertyPrintingConfig<TOwner>
    {
        PropertyInfo PropertyInfo { get; }
    }
}