using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ObjectPrinting.Extensions;

public static class DictionaryExtensions
{
    public static string ObjectPrint<T>(
        this IDictionary dictionary, 
        Serializer<T> serializer,
        ImmutableList<object> previous)
    {
        previous = previous.Add(dictionary);

        return dictionary
            .CastToDictionaryEntries()
            .ObjectPrintingSerialize(
                "{", "}", 
                pair => $"{pair.Key}: {serializer.PrintToString(pair.Value, previous)};", 
                previous.Count);
    }

    private static IEnumerable<DictionaryEntry> CastToDictionaryEntries(this IDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
            yield return entry;
    }
}