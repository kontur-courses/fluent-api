using System;
using ObjectPrinting.Solved;

namespace ObjectPrinting.PrintingConfig;

public interface IPropertyPrintingConfig<TOwner, out TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print);
    
}