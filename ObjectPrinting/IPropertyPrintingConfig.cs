using System.Reflection;
using System.Globalization;

namespace ObjectPrinting
{
    interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo PropertyInfo { get; }
        PrintingConfig<TOwner> SetCulture<T>(CultureInfo culture);
    }
}
