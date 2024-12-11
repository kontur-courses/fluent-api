using System;
using System.Reflection;

namespace ObjectPrinting.Configurations;

public class PropertyPrintingConfig<TOwner, TPropType>(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
{
    public PrintingConfig<TOwner> ParentConfig { get; } = printingConfig;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        return ParentConfig.AddPropSerialize(propertyInfo, print);
    }
}