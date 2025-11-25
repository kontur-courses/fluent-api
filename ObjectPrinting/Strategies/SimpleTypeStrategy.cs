using System;
using System.Text;

namespace ObjectPrinting.Strategies;

public class SimpleTypeStrategy : IPrintStrategy
{
    public bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config)
        => type.IsPrimitive || obj is string || type.GetProperties().Length == 0;

    public void Print<TOwner>(object obj, Type type, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
        sb.Append(obj);
    }
}