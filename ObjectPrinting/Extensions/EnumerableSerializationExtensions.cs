using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Extensions;

public static class EnumerableSerializationExtensions
{
    public static string SerializeEnumerable(this IEnumerable enumerable, int nestingLevel, HashSet<object> visitedObjects, Func<object, int, HashSet<object>, string> serializer)
    {
        var collectionType = enumerable.GetType().Name;
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine($"{collectionType}:");

        foreach (var element in enumerable)
        {
            sb.Append($"{indentation}- {serializer(element, nestingLevel + 1, visitedObjects)}");
        }

        visitedObjects.Remove(enumerable);
        return sb.ToString();
    }
}