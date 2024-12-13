using System;

namespace ObjectPrinting;

public interface IPropertyPrintingConfig<out TOwner, TPropType>
{
    public PrintingConfig<TPropType> Using(Func<TOwner, string> func);
}