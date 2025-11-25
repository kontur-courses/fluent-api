using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace ObjectPrinting.Extensions;

public static class CollectionPrintingExtensions
{
    public static void PrintDictionary<TOwner>(this IDictionary dict, PrintingConfig<TOwner> config, 
        StringBuilder sb, int indent)
    {
        sb.AppendLine(GetTypeName(dict.GetType()) + " {");
        foreach (DictionaryEntry entry in dict)
        {
            sb.Append(new string('\t', indent + 1));
            sb.Append('[');
            entry.Key.Print(config, sb, indent + 1);
            sb.Append("] = ");

            if (entry.Value is null)
            {
                sb.AppendLine("null");
            }
            else
            {
                entry.Value.Print(config, sb, indent + 1);
                sb.AppendLine();
            }
        }

        sb.Append(new string('\t', indent));
        sb.Append('}');
    }

    public static void PrintEnumerable<TOwner>(this IEnumerable enumerable, PrintingConfig<TOwner> config,
        StringBuilder sb, int indent)
    {
        sb.AppendLine(GetTypeName(enumerable.GetType()) + " [");
        foreach (var item in enumerable)
        {
            sb.Append(new string('\t', indent + 1));
            if (item is null)
            {
                sb.AppendLine("null");
            }
            else
            {
                item.Print(config, sb, indent + 1);
                sb.AppendLine();
            }
        }

        sb.Append(new string('\t', indent));
        sb.Append(']');
    }
    
    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var name = type.Name;
        var backtickIndex = name.IndexOf('`');
        if (backtickIndex > 0)
            name = name[..backtickIndex];

        var genericArgs = type.GetGenericArguments()
            .Select(GetTypeName);

        return $"{name}<{string.Join(", ", genericArgs)}>";
    }
}