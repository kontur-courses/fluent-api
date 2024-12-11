using System;

namespace ObjectPrinting;

public interface ITypePrintingConfig<TOwner, out TPropType>
{
    public PrintingConfig<TOwner> Using(Func<TPropType, string> func);
}