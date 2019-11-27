using System.Reflection;

namespace ObjectPrinting
{
    public interface IConcretePropertyPrintingConfig
    {
        PropertyInfo PropertyInfo { get; }
    }
}