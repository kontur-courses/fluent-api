using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ObjectPrinting.Extensions;

public static class EnumerableExtensions
{
    public static string ObjectPrint<T>(
        this IEnumerable<object> enumerable, 
        Serializer<T> serializer,
        ImmutableList<object> previous)
    {
        
        previous = previous.Add(enumerable);

        return enumerable.ObjectPrintingSerialize(
            "[", "]",
            obj => $"{serializer.PrintToString(obj, previous)},",
            previous.Count);
    }

    public static string ObjectPrintingSerialize<T>(
        this IEnumerable<T> enumerable,
        string startSymbol,
        string endSymbol,
        Func<T, string> elementSerializer,
        int indentationLevel)
    {
        var sb = new StringBuilder();
        sb.AppendLine(startSymbol);
        
        var indentation = new string('\t', indentationLevel);
        
        foreach (var obj in enumerable)
        {
            sb.Append(indentation);
            sb.AppendLine(elementSerializer(obj));
        }

        sb.Append(new string('\t', indentationLevel - 1));
        sb.Append(endSymbol);

        return sb.ToString();
    }
}