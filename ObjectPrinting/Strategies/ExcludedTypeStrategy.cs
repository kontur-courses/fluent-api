using System;
using System.Text;

namespace ObjectPrinting.Strategies;

public class ExcludedTypeStrategy : IPrintStrategy
{
    public bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config)
        => config.ExcludedTypes.Contains(type);

    public void Print<TOwner>(object obj, Type type, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
    }
}