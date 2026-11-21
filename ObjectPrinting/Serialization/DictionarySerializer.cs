using System.Collections;
using System.Text;

namespace ObjectPrinting.Serialization;

internal class DictionarySerializer : ISerializationStrategy
{
    public bool CanSerialize(object obj)
    {
        return obj is IDictionary;
    }

    public string Serialize(object obj, int nestingLevel, ObjectSerializer serializer)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(obj.GetType().Name);

        foreach (DictionaryEntry entry in (IDictionary)obj)
        {
            if (serializer.IsTypeExcluded(entry.Key.GetType()) ||
                (entry.Value is not null && serializer.IsTypeExcluded(entry.Value.GetType())))
                continue;

            var key = serializer.Serialize(entry.Key, nestingLevel + 1).TrimEnd('\r', '\n');
            var value = serializer.Serialize(entry.Value, nestingLevel + 1);

            sb.Append(indentation)
                .Append('[')
                .Append(key)
                .Append("] = ")
                .Append(value);
        }

        return sb.ToString();
    }
}