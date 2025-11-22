using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class Printer
{
    private readonly PrintingContext context;

    private readonly HashSet<Type> finalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    ];

    private readonly HashSet<object?> visitedObjects = [];

    public Printer(PrintingContext context)
    {
        this.context = context;
    }

    public string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
            return "null";
        if (finalTypes.Contains(obj.GetType()) || obj.GetType().IsEnum)
            return $"{obj}";
        if (!visitedObjects.Add(obj))
            return "<cyclic reference>";

        return SerializeObject(obj, nestingLevel);
    }

    private string SerializeObject(object obj, int nestingLevel)
    {
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        var indentation = new string('\t', nestingLevel + 1);

        if (obj is IDictionary dict)
            return SerializeDictionary(dict, nestingLevel, indentation);

        if (obj is IEnumerable enumerable and not string)
            return SerializeCollection(enumerable, nestingLevel, indentation);

        sb.Append(SerializeMembers(obj, type.GetProperties(), indentation, nestingLevel));
        sb.Append(SerializeMembers(obj, type.GetFields(), indentation, nestingLevel));

        visitedObjects.Remove(obj);
        return sb.ToString().Trim();
    }

    private string SerializeMembers(object obj, IEnumerable<MemberInfo> memberInfo, string indentation,
        int nestingLevel)
    {
        var sb = new StringBuilder();
        foreach (var propertyInfo in memberInfo)
            if (TrySerializeMember(obj, propertyInfo, indentation, nestingLevel, out var serializedProperty))
                sb.Append(serializedProperty);

        return sb.ToString();
    }

    private bool TrySerializeMember(object obj, MemberInfo memberInfo, string indentation, int nestingLevel,
        out string? serializedProperty)
    {
        var memberType = memberInfo switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new ArgumentException("Member must be property or field")
        };

        var value = memberInfo switch
        {
            PropertyInfo p => p.GetValue(obj),
            FieldInfo f => f.GetValue(obj),
            _ => null
        };

        if (context.excludedMembers.Contains(memberInfo))
        {
            serializedProperty = null;
            return false;
        }

        if (context.excludedTypes.Contains(memberType))
        {
            serializedProperty = null;
            return false;
        }

        var valueType = value?.GetType() ?? typeof(object);
        if (value != null && context.memberSerializers.TryGetValue(memberInfo, out var memberSerializer))
        {
            serializedProperty = FormatLine(indentation, memberInfo.Name, memberSerializer(value));
            return true;
        }

        if (value != null && context.typeSerializers.TryGetValue(valueType, out var typeSerializer))
        {
            serializedProperty = FormatLine(indentation, memberInfo.Name, typeSerializer(value));
            return true;
        }

        if (context.memberCultures.TryGetValue(memberInfo, out var memberCulture) && value is IFormattable f1)
        {
            serializedProperty = FormatLine(
                indentation,
                memberInfo.Name,
                f1.ToString(null, memberCulture));

            return true;
        }

        if (context.typeCultures.TryGetValue(valueType, out var typeCulture) && value is IFormattable f2)
        {
            serializedProperty = FormatLine(
                indentation,
                memberInfo.Name,
                f2.ToString(null, typeCulture));

            return true;
        }

        serializedProperty = FormatLine(
            indentation,
            memberInfo.Name,
            PrintToString(value, nestingLevel + 1));

        return true;
    }

    private string SerializeDictionary(IDictionary dict, int nestingLevel, string indentation)
    {
        var sb = new StringBuilder();
        sb.AppendLine(dict.GetType().Name);

        foreach (DictionaryEntry entry in dict)
        {
            sb.Append(indentation + "[");

            if (finalTypes.Contains(entry.Key.GetType()))
                sb.Append(entry.Key);
            else
                sb.Append(PrintToString(entry.Key, nestingLevel + 1));

            sb.Append("] = ");

            if (entry.Value == null)
                sb.AppendLine("null");
            else if (finalTypes.Contains(entry.Value.GetType()))
                sb.AppendLine(entry.Value.ToString());
            else
                sb.AppendLine(PrintToString(entry.Value, nestingLevel + 1));
        }

        return sb.ToString().Trim();
    }


    private string SerializeCollection(IEnumerable collection, int nestingLevel, string indentation)
    {
        var sb = new StringBuilder();
        sb.AppendLine(collection.GetType().Name);
        var index = 0;

        foreach (var item in collection)
        {
            if (finalTypes.Contains(item.GetType()))
                sb.AppendLine($"{indentation}[{index}] = {item}");
            else
                sb.AppendLine($"{indentation}[{index}] = {PrintToString(item, nestingLevel + 1)}");

            index++;
        }

        return sb.ToString().Trim();
    }

    private string FormatLine(string indentation, string name, string value)
    {
        return $"{indentation}{name} = {value}{Environment.NewLine}";
    }
}