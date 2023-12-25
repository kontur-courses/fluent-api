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
        // var sb = new StringBuilder("[");
        // sb.Append(Environment.NewLine);
        //
        // var identation = new string('\t', previous.Count);
        // previous = previous.Add(enumerable);
        //
        // foreach (var obj in enumerable)
        // {
        //     sb.Append(identation);
        //     sb.Append(printingConfig.PrintToString1(obj, previous));
        //     sb.Append(',');
        //     sb.Append(Environment.NewLine);
        // }
        //
        // sb.Append(new string('\t', previous.Count - 1));
        // sb.Append(']');
        //
        // return sb.ToString();
        
        previous = previous.Add(enumerable);

        return enumerable.EnumerateForObjectPrinting(
            "{", "}",
            obj => printingConfig.PrintToString1(obj, previous),
            previous);
    }

    public static string EnumerateForObjectPrinting<T>(
        this IEnumerable<T> enumerable,
        string startSymbol,
        string endSymbol,
        Func<T, string> getStringElement,
        ImmutableList<object> previous)
    {
        var sb = new StringBuilder();
        sb.AppendLine(startSymbol);
        
        var identation = new string('\t', previous.Count - 1);
        
        foreach (var obj in enumerable)
        {
            sb.Append(identation);
            sb.Append(getStringElement(obj));
            sb.AppendLine(",");
        }

        sb.Append(new string('\t', previous.Count - 1));
        sb.AppendLine(endSymbol);

        return sb.ToString();
    }
}