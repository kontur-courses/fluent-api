using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ObjectPrinting.Extensions;

public static class DictionaryExtensions
{
    public static string ObjectPrintDictionary<T>(
        this IDictionary<object, object> dictionary, 
        PrintingConfig<T> printingConfig,
        ImmutableList<object> previous)
    {
        // var sb = new StringBuilder("{");
        // sb.Append(Environment.NewLine);
        //
        // var identation = new string('\t', previous.Count);
        // previous = previous.Add(dictionary);
        //
        // foreach (var (key, value) in dictionary)
        // {
        //     sb.Append(identation);
        //     sb.Append(key);
        //     sb.Append(": ");
        //     sb.Append(printingConfig.PrintToString1(value, previous));
        //     sb.Append(',');
        //     sb.Append(Environment.NewLine);
        // }
        //
        // sb.Append(new string('\t', previous.Count - 1));
        // sb.Append('}');
        //
        // return sb.ToString();
        previous = previous.Add(dictionary);

        return dictionary.AsEnumerable().EnumerateForObjectPrinting(
            "{", "}",
            pair => $"{pair.Key}: {printingConfig.PrintToString1(pair.Value, previous)}",
            previous);
    }
}