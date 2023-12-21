using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = new();
    private readonly Dictionary<Type, Func<object, string>> customSerialization = new();

    private readonly Type[] finalTypes =
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    public PrintingConfig<TOwner> ExcludeType<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> WithSerializationForType<T>(Func<T, string> serializationFunc)
    {
        var type = typeof(T);
        customSerialization[type] = obj => serializationFunc((T) obj);
        return this;
    }

    // private string ApplyCustomSerializationIfPresent(object obj, string initial)
    // {
    //     return customSerialization.TryGetValue(obj.GetType(), out var serialization) 
    //         ? serialization(obj) 
    //         : initial;
    // }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj is null)
            return "null" + Environment.NewLine;

        if (excludedTypes.Contains(obj.GetType()))
            return "";

        if (customSerialization.TryGetValue(obj.GetType(), out var serialization))
            return serialization(obj) + Environment.NewLine;
        
        if (finalTypes.Contains(obj.GetType()))
            return obj + Environment.NewLine;

        var printedProperties = PrintObjectProperties(obj, nestingLevel);

        return printedProperties;
    }

    private string PrintObjectProperties(object obj, int nestingLevel)
    {
        var identation = new string('\t', nestingLevel + 1);
        var objectType = obj.GetType();
        var sb = new StringBuilder().AppendLine(objectType.Name);

        foreach (var propertyInfo in objectType.GetProperties())
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType))
                continue;

            sb.Append(identation + propertyInfo.Name + " = " +
                      PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
        }

        return sb.ToString();
    }
}