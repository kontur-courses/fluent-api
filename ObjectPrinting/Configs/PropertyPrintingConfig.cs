using System;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropertyType>
{
    public PrintingConfig<TOwner> Using(Func<TPropertyType, string> print)
    {
        throw new NotImplementedException();
    }
}