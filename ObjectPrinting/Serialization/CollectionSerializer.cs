using System.Collections;
using System.Text;

namespace ObjectPrinting.Serialization;

internal class CollectionSerializer : ISerializationStrategy
{
    public bool CanSerialize(object obj)
    {
        return obj is IEnumerable;
    }

    public string Serialize(object obj, int nestingLevel, ObjectSerializer serializer)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(obj.GetType().Name);

        var index = 0;
        foreach (var item in (IEnumerable)obj)
        {
            if (item is not null && serializer.IsTypeExcluded(item.GetType()))
            {
                index++;
                continue;
            }

            sb.Append(indentation)
                .Append('[')
                .Append(index)
                .Append("] = ")
                .Append(serializer.Serialize(item, nestingLevel + 1));

            index++;
        }

        return sb.ToString();
    }
}