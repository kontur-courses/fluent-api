using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    internal PrintingConfig<TOwner> PrintingConfig { get; }
    internal readonly PropertyInfo? PropertyInfo;


    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
    {
        PrintingConfig = printingConfig;
        PropertyInfo = propertyInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> newSerialize)
    {
        return PropertyInfo == null ? SetSerializedType(newSerialize) : SetSerializedProp(newSerialize);
    }

    public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
    {
        PrintingConfig.AddAlternativeCulture(typeof(TPropType), cultureInfo);
        return PrintingConfig;
    }

    private PrintingConfig<TOwner> SetSerializedProp(Func<TPropType, string> newSerialize)
    {
        PrintingConfig.AddSerializedProperty(PropertyInfo!, newSerialize);
        return PrintingConfig;
    }

    private PrintingConfig<TOwner> SetSerializedType(Func<TPropType, string> newSerialize)
    {
        PrintingConfig.AddSerializedType(newSerialize);
        return PrintingConfig;
    }
}