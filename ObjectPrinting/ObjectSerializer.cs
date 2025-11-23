using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

public class ObjectSerializer
{
    private readonly MemberPrinter memberPrinter = new MemberPrinter();
    private readonly EnumerablePrinter enumerablePrinter = new EnumerablePrinter();

    public string Print<TOwner>(TOwner obj, PrintingConfig<TOwner> config)
    {
        var visited = new HashSet<object>(new ReferenceEqualityComparer());
        return PrintInternal(obj, 0, config, visited);
    }

    internal string PrintInternal(
        object obj,
        int level,
        IPrintingConfigInternal config,
        HashSet<object> visited)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (TypeHelper.IsSimpleType(type))
            return obj + Environment.NewLine;

        if (!type.IsValueType && !visited.Add(obj))
            return $"<cyclic reference to {type.Name}>" + Environment.NewLine;

        if (obj is IEnumerable enumerable)
            return enumerablePrinter.PrintEnumerable(enumerable, type, level, config, visited, this);

        var indent = new string('\t', level);
        var sb = new StringBuilder();

        sb.AppendLine(type.Name);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0) continue;
            if (config.ExcludedTypes.Contains(property.PropertyType)) continue;
            if (config.ExcludedProperties.Contains(property.Name)) continue;

            var value = property.GetValue(obj);
            sb.Append(indent + "\t" +
                      memberPrinter.PrintMember(property.Name, value, property.PropertyType, config, level + 1, visited, this));
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (config.ExcludedTypes.Contains(field.FieldType)) continue;
            if (config.ExcludedProperties.Contains(field.Name)) continue;

            var value = field.GetValue(obj);
            sb.Append(indent + "\t" +
                      memberPrinter.PrintMember(field.Name, value, field.FieldType, config, level + 1, visited, this));
        }

        return sb.ToString();
    }
}
