using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> PrintingConfig { get; }
    public PropertyInfo PropertyInfo { get; }

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
    {
        this.PrintingConfig = printingConfig;
        this.PropertyInfo = propertyInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        var castedFunc = new Func<object, string>((obj) => print((TPropType)obj));
        PrintingConfig.propertySerializers.Add(PropertyInfo, castedFunc);
        return PrintingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => PrintingConfig;
}

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}