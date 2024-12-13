using System;
using System.Globalization;
using System.Reflection;
using ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> parentConfig,
    PropertyInfo propertyInfo)
{
    public readonly PrintingConfig<TOwner> parentConfig = parentConfig;
    public readonly PropertyInfo propertyInfo = propertyInfo;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        parentConfig.AddPropertySerializer(propertyInfo, obj => print((TPropType)obj));
        return parentConfig;
    }

    public PrintingConfig<TOwner> WithCulture(CultureInfo culture)
    {
        parentConfig.AddPropertyCulture(propertyInfo, culture);
        return parentConfig;
    }

    public PrintingConfig<TOwner> Exclude()
    {
        parentConfig.AddExcludedProperty(propertyInfo);
        return parentConfig;
    }
}