using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectPrinting.Strategies;

namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    private static readonly List<IPrintStrategy> Strategies = new()
    {
        new ExcludedTypeStrategy(),
        new TypeSerializerStrategy(),
        new DictionaryStrategy(),
        new EnumerableStrategy(),
        new SimpleTypeStrategy(),
        new DefaultObjectStrategy()
    };

    public static void Print<TOwner>(this object obj, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
        if (obj is null)
        {
            sb.Append("null");
            return;
        }
        
        if (!config.Visited.Add(obj))
        {
            sb.Append("<cyclic reference>");
            return;
        }

        var type = obj.GetType();

        foreach (var strategy in Strategies.Where(strategy => strategy.CanPrint(obj, type, config)))
        {
            strategy.Print(obj, type, config, sb, indent);
            return;
        }
    }
}