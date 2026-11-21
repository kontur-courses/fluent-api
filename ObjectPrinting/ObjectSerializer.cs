using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

internal class ObjectSerializer
{
    private static readonly HashSet<Type> FinalTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateOnly),
        typeof(TimeOnly)
    ];

    private readonly PrintingConfigState state;

    public ObjectSerializer(PrintingConfigState state)
    {
        this.state = state;
    }

    public string Serialize(object? obj)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return Serialize(obj, 0, null, visited);
    }

    private string Serialize(
        object? obj,
        int nestingLevel,
        MemberInfo? member,
        HashSet<object> visited)
    {
        if (obj is null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (TrySerializeCustom(obj, type, member, out var customSerialized))
            return customSerialized + Environment.NewLine;

        if (IsFinalType(type))
            return FormatFinalValue(obj, member) + Environment.NewLine;

        if (!type.IsValueType && !visited.Add(obj))
            return "<cyclic reference>" + Environment.NewLine;

        try
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            foreach (var (memberInfo, memberType, value) in GetMembers(obj, type))
            {
                if (IsExcluded(memberInfo, memberType))
                    continue;

                sb.Append(indentation)
                    .Append(memberInfo.Name)
                    .Append(" = ")
                    .Append(Serialize(value, nestingLevel + 1, memberInfo, visited));
            }

            return sb.ToString();
        }
        finally
        {
            if (!type.IsValueType)
                visited.Remove(obj);
        }
    }

    private bool TrySerializeCustom(
        object obj,
        Type type,
        MemberInfo? member,
        out string result)
    {
        if (member is not null)
        {
            if (state.MemberSerializers.TryGetValue(member, out var memberSerializer))
            {
                result = memberSerializer(obj);
                if (obj is string && state.MemberMaxLengths.TryGetValue(member, out var memberMaxLen))
                    result = TruncateString(result, memberMaxLen);
                return true;
            }

            if (state.MemberCultures.TryGetValue(member, out var memberCulture) && obj is IFormattable memberFormattable)
            {
                result = memberFormattable.ToString(null, memberCulture);
                return true;
            }

            if (obj is string memberStr && state.MemberMaxLengths.TryGetValue(member, out var maxLen))
            {
                result = TruncateString(memberStr, maxLen);
                return true;
            }
        }

        if (state.TypeSerializers.TryGetValue(type, out var typeSerializer))
        {
            result = typeSerializer(obj);
            if (obj is string && state.StringMaxLength.HasValue)
                result = TruncateString(result, state.StringMaxLength.Value);
            return true;
        }

        if (state.TypeCultures.TryGetValue(type, out var typeCulture) && obj is IFormattable typeFormattable)
        {
            result = typeFormattable.ToString(null, typeCulture);
            return true;
        }

        result = string.Empty;
        return false;
    }

    private string FormatFinalValue(object obj, MemberInfo? member)
    {
        if (obj is string str)
        {
            if (member is not null && state.MemberMaxLengths.TryGetValue(member, out var memberMaxLen))
                return TruncateString(str, memberMaxLen);

            if (state.StringMaxLength.HasValue)
                return TruncateString(str, state.StringMaxLength.Value);
        }

        return obj.ToString() ?? string.Empty;
    }

    private static string TruncateString(string value, int maxLength) =>
        value.Length > maxLength ? value[..maxLength] : value;

    private bool IsExcluded(MemberInfo member, Type memberType) =>
        state.ExcludedMembers.Contains(member) || state.ExcludedTypes.Contains(memberType);

    private static bool IsFinalType(Type type) =>
        type.IsPrimitive || type.IsEnum || FinalTypes.Contains(type);

    private static IEnumerable<(MemberInfo Member, Type MemberType, object? Value)> GetMembers(object obj, Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        foreach (var property in type.GetProperties(flags))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
                continue;

            yield return (property, property.PropertyType, property.GetValue(obj));
        }

        foreach (var field in type.GetFields(flags))
        {
            yield return (field, field.FieldType, field.GetValue(obj));
        }
    }
}
