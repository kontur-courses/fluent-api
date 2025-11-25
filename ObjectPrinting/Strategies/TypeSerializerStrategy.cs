using System;
using System.Text;

namespace ObjectPrinting.Strategies;

public class TypeSerializerStrategy : IPrintStrategy
{
    public bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config)
        => config.TypeSerializers.ContainsKey(type);

    public void Print<TOwner>(object obj, Type type, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
        var serializer = config.TypeSerializers[type];
        sb.Append(serializer(obj));
    }
}