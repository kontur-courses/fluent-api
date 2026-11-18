using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

internal class ObjectSerializer<TOwner>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly HashSet<object> visitedObjects = new();

    public ObjectSerializer(PrintingConfig<TOwner> config)
    {
        this.config = config;
    }

    public string Serialize(object? obj)
    {
        return PrintToStringInternal(obj, 0);
    }

    private string? PrintToStringInternal(object? obj, int nestingLevel, string? propertyName = null)
    {
        if (TryGetCustomFormat(obj, propertyName, out string? customResult))
            return customResult;

        var type = obj!.GetType();

        if (HandleCircularReference(obj, type, out string? cycleResult))
            return cycleResult;

        string result;

        if (obj is IEnumerable enumerable)
            result = PrintEnumerable(enumerable, type, nestingLevel, propertyName);
        else
            result = PrintObject(obj, type, nestingLevel, propertyName);

        if (!type.IsValueType)
            visitedObjects.Remove(obj);

        return result;
    }

    private bool TryGetCustomFormat(object? obj, string? propertyName, out string? result)
    {
        result = null;

        if (obj != null && config.excludedTypes.Contains(obj.GetType()))
            return true;

        if (obj == null)
        {
            result = "null" + Environment.NewLine;
            return true;
        }

        var type = obj.GetType();

        if (propertyName != null && config.propertySerializers.TryGetValue(propertyName, out var propSerializer))
        {
            result = propSerializer(obj) + Environment.NewLine;
            return true;
        }

        if (obj is string str && propertyName != null && config.trimLengths.TryGetValue(propertyName, out var length))
        {
            var trimmed = str.Length <= length ? str : str[..length];
            result = trimmed + Environment.NewLine;
            return true;
        }

        if (config.typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            result = typeSerializer(obj) + Environment.NewLine;
            return true;
        }

        if (IsFinalType(type))
        {
            result = SerializeValue(obj, type) + Environment.NewLine;
            return true;
        }

        return false;
    }

    private bool HandleCircularReference(object obj, Type type, out string? result)
    {
        result = null;
        if (type.IsValueType)
            return false;

        if (visitedObjects.Contains(obj))
        {
            result = "<Circular Reference>" + Environment.NewLine;
            return true;
        }

        visitedObjects.Add(obj);
        return false;
    }

    private string PrintEnumerable(IEnumerable enumerable, Type type, int nestingLevel, string? propertyName)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        foreach (var item in enumerable)
        {
            var itemStr = PrintToStringInternal(item, nestingLevel + 1, propertyName);
            if (itemStr == null)
                continue;

            sb.Append(indentation);
            sb.Append(itemStr.TrimEnd('\r', '\n'));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string PrintObject(object obj, Type type, int nestingLevel, string? parentPath = null)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field)
            .Where(m => !(m is PropertyInfo p && p.GetIndexParameters().Length > 0));

        foreach (var member in members)
        {
            string currentPath = parentPath == null ? member.Name : $"{parentPath}.{member.Name}";

            if (config.excludedProperties.Contains(currentPath) || config.excludedTypes.Contains(GetMemberType(member)))
                continue;

            object? value = member switch
            {
                PropertyInfo p => p.GetValue(obj),
                FieldInfo f => f.GetValue(obj),
                _ => null
            };

            var rawValue = PrintToStringInternal(value, nestingLevel + 1, currentPath);
            if (rawValue == null)
                continue;

            string valueStr = rawValue.TrimEnd('\r', '\n');
            sb.Append(indentation + member.Name + " = " + valueStr);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string SerializeValue(object obj, Type type)
    {
        if (config.typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
            return formattable.ToString(null, culture);

        if (obj is IFormattable f)
            return f.ToString(null, CultureInfo.InvariantCulture);

        return obj.ToString()!;
    }

    private bool IsFinalType(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
               type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum;
    }

    private Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new NotSupportedException()
        };
    }
}