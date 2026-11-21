using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Serialization;

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

    private static readonly ISerializationStrategy[] DefaultStrategies =
    [
        new DictionarySerializer(),
        new CollectionSerializer(),
        new ComplexObjectSerializer()
    ];

    private readonly PrintingConfigState state;
    private readonly IReadOnlyList<ISerializationStrategy> strategies;
    private readonly HashSet<object> visited = new(ReferenceEqualityComparer.Instance);

    public ObjectSerializer(PrintingConfigState state)
        : this(state, DefaultStrategies)
    {
    }

    internal ObjectSerializer(PrintingConfigState state, IReadOnlyList<ISerializationStrategy> strategies)
    {
        this.state = state;
        this.strategies = strategies;
    }

    public string Serialize(object? obj)
    {
        visited.Clear();
        return Serialize(obj, 0);
    }

    internal string Serialize(object? obj, int nestingLevel, MemberInfo? member = null)
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
            var strategy = strategies.First(s => s.CanSerialize(obj));
            return strategy.Serialize(obj, nestingLevel, this);
        }
        finally
        {
            if (!type.IsValueType)
                visited.Remove(obj);
        }
    }

    internal bool IsExcluded(MemberInfo member, Type memberType)
    {
        return state.ExcludedMembers.Contains(member) || state.ExcludedTypes.Contains(memberType);
    }

    internal bool IsTypeExcluded(Type type)
    {
        return state.ExcludedTypes.Contains(type);
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

            if (state.MemberCultures.TryGetValue(member, out var memberCulture) &&
                obj is IFormattable memberFormattable)
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

    private static string TruncateString(string value, int maxLength)
    {
        return value.Length > maxLength ? value[..maxLength] : value;
    }

    private static bool IsFinalType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || FinalTypes.Contains(type);
    }
}