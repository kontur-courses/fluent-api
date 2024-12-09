using System;
using System.Globalization;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropertyType>(PrintingConfig<TOwner> headConfig)
{
    public PrintingConfig<TOwner> HeadConfig { get; } = headConfig;
    
    public PrintingConfig<TOwner> Using(Func<TPropertyType, string> printing)
    {
        HeadConfig.TypeSerializers[typeof(TPropertyType)] = p => printing((TPropertyType)p);
        
        return HeadConfig;
    }
}

