using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : IPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> parent;
    private readonly Dictionary<PropertyInfo, Func<object, string>> customSerializers;
    private readonly PropertyInfo propertyInfo;

    public PropertyPrintingConfig(PrintingConfig<TOwner> parent, 
        Dictionary<PropertyInfo, Func<object, string>> customSerializers,
        PropertyInfo propertyInfo)
    {
        this.parent = parent;
        this.customSerializers = customSerializers;
        this.propertyInfo = propertyInfo;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        customSerializers[propertyInfo] = obj => print((TPropType)obj);
        return parent;
    }
}