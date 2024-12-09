using System;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TProperty>(PrintingConfig<TOwner> headConfig, PropertyInfo propertyInfo)
{
    public PrintingConfig<TOwner> HeadConfig => headConfig;
    public PropertyInfo PropertyInfo => propertyInfo;
    
    public PrintingConfig<TOwner> Using(Func<TProperty, string> printing)
    {
        HeadConfig.PropertySerializers[propertyInfo] = p => printing((TProperty)p);
        
        return HeadConfig;
    }
}