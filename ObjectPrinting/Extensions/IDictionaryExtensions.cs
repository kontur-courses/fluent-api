using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ObjectPrinting.Extensions;

public static class DictionaryExtensions
{
    public static string ObjectPrintDictionary<T>(
        this IDictionary dictionary, 
        PrintingConfig<T> printingConfig,
        ImmutableList<object> previous)
    {
        previous = previous.Add(dictionary);

        return dictionary
            .KeysToValues()
            .EnumerateForObjectPrinting(
                "{", "}", 
                pair => $"{pair.Key}: {printingConfig.PrintToString1(pair.Value, previous)};", 
                previous);
    }

    private static IEnumerable<DictionaryEntry> KeysToValues(this IDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
            yield return entry;
    }
}