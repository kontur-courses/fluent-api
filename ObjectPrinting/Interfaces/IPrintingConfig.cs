using System;

namespace ObjectPrinting.Interfaces;

public interface IPrintingConfig<TOwner, TType>
{
    public PrintingConfig<TOwner> Using(Func<TType, string> print);
}