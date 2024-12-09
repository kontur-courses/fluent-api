using System;

namespace ObjectPrinting.Solved;

public interface IPropertyPrintingConfig<TOwner, out TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
    
}