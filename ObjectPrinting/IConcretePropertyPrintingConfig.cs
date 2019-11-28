using System.Reflection;

namespace ObjectPrinting
{
    public interface IConcretePropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        PropertyInfo PropertyInfo { get; }
    }
}