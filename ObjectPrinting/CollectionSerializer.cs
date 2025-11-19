using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    internal static class CollectionSerializer
    {
        public static bool IsCollection(Type type)
        {
            if (type.IsArray)
                return true;

            return typeof(IEnumerable).IsAssignableFrom(type) && 
                   type != typeof(string);
        }

        public static bool IsDictionary(Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
                return true;

            return type.IsGenericType && 
                   (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                    type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static string SerializeCollection(object collection, int nestingLevel, HashSet<object> visitedObjects, PrintingConfigData config)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var collectionType = collection.GetType();

            sb.AppendLine("[");

            var enumerable = (IEnumerable)collection;
            var items = enumerable.Cast<object>().ToList();
            var maxItems = config.CollectionMaxItems.GetValueOrDefault(collectionType, int.MaxValue);

            foreach (var item in items.Take(maxItems))
            {
                string serializedItem;
                if (config.CollectionSerializers.TryGetValue(collectionType, out var serializer))
                {
                    serializedItem = serializer(item);
                }
                else
                {
                    var objectSerializer = new ObjectSerializer(config);
                    serializedItem = objectSerializer.PrintToString(item, nestingLevel + 1, visitedObjects).TrimEnd();
                }

                sb.AppendLine(identation + serializedItem + ",");
            }

            if (items.Count > maxItems)
            {
                var remaining = items.Count - maxItems;
                sb.AppendLine(identation + $"... and {remaining} more items");
            }
            else if (items.Count == 0)
            {
                sb.AppendLine(identation + "empty");
            }

            sb.AppendLine("]");
            return sb.ToString();
        }

        public static string SerializeDictionary(object dictionary, int nestingLevel, HashSet<object> visitedObjects, PrintingConfigData config)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var dictType = dictionary.GetType();

            sb.AppendLine("[");

            try
            {
                var dict = (IDictionary)dictionary;
                var count = 0;
                var maxItems = config.CollectionMaxItems.GetValueOrDefault(dictType, int.MaxValue);
                var objectSerializer = new ObjectSerializer(config);

                foreach (DictionaryEntry entry in dict)
                {
                    if (count >= maxItems)
                    {
                        var remaining = dict.Count - maxItems;
                        sb.AppendLine(identation + $"... and {remaining} more items");
                        break;
                    }

                    var serializedKey = objectSerializer.PrintToString(entry.Key, nestingLevel + 1, visitedObjects).Trim();
                    var serializedValue = objectSerializer.PrintToString(entry.Value, nestingLevel + 1, visitedObjects).Trim();
                    
                    serializedKey = serializedKey.Replace(Environment.NewLine, " ").Trim();
                    serializedValue = serializedValue.Replace(Environment.NewLine, " ").Trim();
                    
                    sb.AppendLine(identation + $"{serializedKey}: {serializedValue},");
                    count++;
                }

                if (count == 0)
                {
                    sb.AppendLine(identation + "empty");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(identation + $"Error: {ex.Message}");
            }

            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}