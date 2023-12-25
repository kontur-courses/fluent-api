using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;
    private readonly PropertyInfo propertyInfo;

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo property)
    {
        this.printingConfig = printingConfig;
        propertyInfo = property;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        var config = (IPrintingConfig<TOwner>)printingConfig;
        config.PropertySerializers[propertyInfo] = p => print((TPropType)p);
        
        return printingConfig;
    }

    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

    PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.Property => propertyInfo;
}