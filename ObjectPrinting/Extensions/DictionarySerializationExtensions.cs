using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Extensions;

public static class DictionarySerializationExtensions
{
    public static string SerializeDictionary(this IDictionary dictionary, int nestingLevel, HashSet<object> visitedObjects, Func<object, int, HashSet<object>, string> serializer)
    {
        var dictionaryType = dictionary.GetType().Name;
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine($"{dictionaryType}:");

        foreach (DictionaryEntry entry in dictionary)
        {
            sb.Append($"{indentation}Key = {serializer(entry.Key, nestingLevel + 1, visitedObjects)}");
            sb.Append($"{indentation}Value = {serializer(entry.Value, nestingLevel + 1, visitedObjects)}");
        }

        visitedObjects.Remove(dictionary);
        return sb.ToString();
    }
}