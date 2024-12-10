using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    public readonly PrintingConfig<TOwner> PrintingConfig;
    public readonly PropertyInfo PropertyInfo;


    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
    {
        PrintingConfig = printingConfig;
        PropertyInfo = propertyInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> newSerialize)
    {
        PrintingConfig.AddSerializedProperty(PropertyInfo, newSerialize);
        return PrintingConfig;
    }
}