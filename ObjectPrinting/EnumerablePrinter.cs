using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

internal class EnumerablePrinter
{
    public string PrintEnumerable(
        IEnumerable enumerable,
        Type type,
        int level,
        IPrintingConfigInternal config,
        HashSet<object> visited,
        ObjectSerializer serializer)
    {
        var indent = new string('\t', level);
        var sb = new StringBuilder();

        sb.AppendLine(type.Name);

        if (enumerable is IDictionary dict)
        {
            foreach (DictionaryEntry entry in dict)
            {
                sb.Append(indent + "\t");
                sb.Append($"[{entry.Key}] = {serializer.PrintInternal(entry.Value, level + 1, config, visited)}");
            }
        }
        else
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                sb.Append(indent + "\t");

                if (item != null &&
                    item.GetType().IsGenericType &&
                    item.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    dynamic kv = item;
                    sb.Append($"[{kv.Key}] = {serializer.PrintInternal(kv.Value, level + 1, config, visited)}");
                }
                else
                {
                    sb.Append($"[{i}] = {serializer.PrintInternal(item, level + 1, config, visited)}");
                }

                i++;
            }
        }

        return sb.ToString();
    }
}