using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ObjectPrinting.Extensions;

public static class EnumerableExtensions
{
    public static string ObjectPrintEnumerable<T>(
        this IEnumerable<object> enumerable, 
        PrintingConfig<T> printingConfig,
        ImmutableList<object> previous)
    {
        
        previous = previous.Add(enumerable);

        return enumerable.EnumerateForObjectPrinting(
            "[", "]",
            obj => $"{printingConfig.PrintToString1(obj, previous)},",
            previous);
    }

    public static string EnumerateForObjectPrinting<T>(
        this IEnumerable<T> enumerable,
        string startSymbol,
        string endSymbol,
        Func<T, string> getRow,
        ImmutableList<object> previous)
    {
        var sb = new StringBuilder();
        sb.AppendLine(startSymbol);
        
        var identation = new string('\t', previous.Count);
        
        foreach (var obj in enumerable)
        {
            sb.Append(identation);
            sb.AppendLine(getRow(obj));
        }

        sb.Append(new string('\t', previous.Count - 1));
        sb.Append(endSymbol);

        return sb.ToString();
    }
}