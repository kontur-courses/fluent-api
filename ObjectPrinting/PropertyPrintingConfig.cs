using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo = null)
    : IPropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> Using(Func<TPropType, string> printingMethod)
    {
        if (propertyInfo is null)
            printingConfig.CustomTypeSerializers[typeof(TPropType)] = printingMethod;
        else
            printingConfig.CustomPropertySerializers[propertyInfo] = printingMethod;

        return printingConfig;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        return printingConfig;
    }
    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
}

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}