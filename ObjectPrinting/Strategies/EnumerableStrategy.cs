using System;
using System.Collections;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Strategies;

public class EnumerableStrategy : IPrintStrategy
{
    public bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config)
        => obj is IEnumerable && obj is not string;

    public void Print<TOwner>(object obj, Type type, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
        ((IEnumerable)obj).PrintEnumerable(config, sb, indent);
    }
}