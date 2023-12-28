using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public class Serializer<T> : ISerializer<T>
{
    private readonly PrintingConfig<T> printingConfig;
    
    public Serializer(PrintingConfig<T> printingConfig)
    {
        this.printingConfig = printingConfig;
    }

    public string PrintToString(T obj)
    {
        return PrintToString(obj, new List<object>().ToImmutableList());
    }
    
    public string PrintToString(object obj, ImmutableList<object> previous)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();

        if (printingConfig.TrySerializeValueType(type, obj, out var serializedValue))
            return serializedValue;

        if (IsCyclic(obj, previous))
            return "cyclic link";

        if (obj is IDictionary dictionary)
            return dictionary.ObjectPrint(this, previous);

        if (obj is IEnumerable enumerable)
            return enumerable.Cast<object>().ObjectPrint(this, previous);

        var indentation = new string('\t', previous.Count + 1);
        previous = previous.Add(obj);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name + " (");

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (printingConfig.IsExcluded(property))
                continue;
            
            sb.Append($"{indentation}{property.Name}: ");

            if (printingConfig.TrySerializeProperty(obj, property, out var serialized)) 
                sb.Append(serialized);
            else
                sb.Append(PrintToString(property.GetValue(obj), previous));
            
            sb.AppendLine(";");
        }
        
        sb.Append(new string('\t', previous.Count - 1) + ")");

        return sb.ToString();
    }
    
    private static bool IsCyclic(object current, IEnumerable<object> previous)
    {
        return previous.Any(e => ReferenceEquals(e, current));
    }
}