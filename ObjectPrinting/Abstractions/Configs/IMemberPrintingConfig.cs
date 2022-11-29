using System;
using ObjectPrinting.Implementations.Configs;

namespace ObjectPrinting.Abstractions.Configs;

public interface IMemberPrintingConfig<TOwner, out TMember>
{
    public PrintingConfig<TOwner> Using(Func<TMember, string> printFunc);
}