using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public static class ObjectPrinter
{
    public static PrintingConfig<TOwner> Setup<TOwner>() => new();
}

public sealed class ObjectPrinter<TOwner>
{
    private readonly PrintingRules _rules;

    private record MemberAccessor(string Name, Type Type, Func<object, object?> GetValue);

    private static readonly ConcurrentDictionary<Type, MemberAccessor[]> MemberCache = new();

    internal ObjectPrinter(PrintingRules rules)
    {
        _rules = rules;
    }

    public string PrintToString(TOwner? obj)
    {
        if (obj is null)
            return "null" + Environment.NewLine;

        var ancestors = new HashSet<object>(ReferenceEqualityComparer.Instance);

        return FormatValue(obj, MemberPath.Empty, nestingLevel: 0, ancestors);
    }

    private string FormatValue(object value, MemberPath memberPath, int nestingLevel, HashSet<object> ancestors)
    {
        var type = value.GetType();

        if (_rules.ExcludedTypes.Contains(type))
            return string.Empty;

        if (_rules.TypeSerializationRules.TryGetValue(type, out var typeSerializer))
        {
            var serialized = typeSerializer(value);
            return PostProcessString(serialized, memberPath, isStringType: type == typeof(string)) + Environment.NewLine;
        }

        if (value is IFormattable formattable && IsTerminalFormattable(type, memberPath))
        {
            var culture = GetCulture(type, memberPath) ?? CultureInfo.InvariantCulture;
            return formattable.ToString(null, culture) + Environment.NewLine;
        }

        if (value is string str)
            return PostProcessString(str, memberPath, isStringType: true) + Environment.NewLine;

        if (type.IsPrimitive || type.IsEnum)
            return value + Environment.NewLine;

        return PrintComplexObject(value, nestingLevel, ancestors, memberPath);
    }

    private string PrintComplexObject(object obj, int nestingLevel, HashSet<object> ancestors, MemberPath currentPath)
    {
        var type = obj.GetType();

        if (!type.IsValueType)
        {
            if (ancestors.Contains(obj))
                return "[cyclic reference]" + Environment.NewLine;

            ancestors.Add(obj);
        }

        try
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            foreach (var member in GetPrintableMembers(type))
            {
                var memberPath = currentPath.Append(member.Name);

                var printedMember = PrintMember(member, obj, nestingLevel, ancestors, memberPath);
                if (printedMember is not null)
                    sb.Append(indentation + printedMember);
            }

            return sb.ToString();
        }
        finally
        {
            if (!type.IsValueType)
                ancestors.Remove(obj);
        }
    }

    private string? PrintMember(MemberAccessor member, object target, int nestingLevel, HashSet<object> ancestors, MemberPath memberPath)
    {
        if (_rules.ExcludedTypes.Contains(member.Type))
            return null;

        if (_rules.ExcludedMembers.Contains(memberPath))
            return null;

        var value = member.GetValue(target);

        if (value is not null && _rules.ExcludedTypes.Contains(value.GetType()))
            return null;

        if (_rules.MemberSerializationRules.TryGetValue(memberPath, out var memberSerializer))
        {
            var serialized = memberSerializer(value!);
            var isString = member.Type == typeof(string) || value is string;
            return $"{member.Name} = {PostProcessString(serialized, memberPath, isString)}{Environment.NewLine}";
        }

        if (value is null)
            return $"{member.Name} = null{Environment.NewLine}";

        var formattedValue = FormatValue(value, memberPath, nestingLevel + 1, ancestors);
        return $"{member.Name} = {formattedValue}";
    }

    private static MemberAccessor[] GetPrintableMembers(Type type)
    {
        return MemberCache.GetOrAdd(type, static t =>
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var list = new List<MemberAccessor>();

            foreach (var prop in t.GetProperties(flags))
                if (prop.CanRead && prop.GetIndexParameters().Length == 0)
                    list.Add(new MemberAccessor(prop.Name, prop.PropertyType, obj => prop.GetValue(obj)));

            foreach (var field in t.GetFields(flags))
                list.Add(new MemberAccessor(field.Name, field.FieldType, obj => field.GetValue(obj)));

            return [.. list];
        });
    }

    private string PostProcessString(string? text, MemberPath memberPath, bool isStringType)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        int? length = null;

        if (_rules.MemberTruncateRules.TryGetValue(memberPath, out var memberLength))
            length = memberLength;
        else if (isStringType && _rules.TypeTruncateRules.TryGetValue(typeof(string), out var typeLength))
            length = typeLength;

        if (length.HasValue && text.Length > length.Value)
            return text[..length.Value];

        return text;
    }

    private bool IsTerminalFormattable(Type type, MemberPath memberPath)
    {
        if (_rules.MemberCultureRules.ContainsKey(memberPath) || _rules.TypeCultureRules.ContainsKey(type))
            return true;

        return type.Namespace == "System" || type.Namespace == "System.Numerics";
    }

    private CultureInfo? GetCulture(Type type, MemberPath memberPath)
    {
        if (_rules.MemberCultureRules.TryGetValue(memberPath, out var memberCulture))
            return memberCulture;

        if (_rules.TypeCultureRules.TryGetValue(type, out var typeCulture))
            return typeCulture;

        return null;
    }
}