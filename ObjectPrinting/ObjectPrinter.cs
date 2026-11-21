using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public static class ObjectPrinter
{
    public static PrintingConfig<TOwner> Setup<TOwner>() => new();
}

public sealed class ObjectPrinter<TOwner>
{
    private static readonly HashSet<Type> TerminalTypes =
    [
        typeof(string),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(Guid),
        typeof(decimal),
        typeof(BigInteger)
    ];

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

        if (IsTypeExcluded(type))
            return string.Empty;

        if (_rules.TypeSerializationRules.TryGetValue(type, out var typeSerializer))
        {
            var serialized = typeSerializer(value);
            return PostProcessString(serialized, memberPath, isStringType: type == typeof(string)) + Environment.NewLine;
        }

        if (IsTerminal(type, memberPath))
        {
            if (value is IFormattable formattable)
            {
                var culture = GetCulture(type, memberPath) ?? CultureInfo.InvariantCulture;
                return formattable.ToString(null, culture) + Environment.NewLine;
            }

            if (value is string str)
                return PostProcessString(str, memberPath, isStringType: true) + Environment.NewLine;

            return value + Environment.NewLine;
        }

        if (value is IDictionary dictionary)
            return PrintDictionary(dictionary, nestingLevel, ancestors, memberPath);

        if (TryExtractDictionaryEntries(value, out var dictEntries, out var dictCount))
            return PrintDictionaryEntries(value.GetType(), dictEntries, dictCount, nestingLevel, ancestors, memberPath);

        if (value is IEnumerable enumerable)
            return PrintEnumerable(enumerable, nestingLevel, ancestors, memberPath);

        return PrintComplexObject(value, nestingLevel, ancestors, memberPath);
    }

    private const int MaxSequenceElements = 1000;

    private string PrintDictionary(IDictionary dict, int nestingLevel, HashSet<object> ancestors, MemberPath currentPath)
    {
        static IEnumerable<DictionaryEntry> Enumerate(IDictionary d)
        {
            foreach (DictionaryEntry entry in d)
                yield return entry;
        }

        return PrintDictionaryCore(dict, dict.GetType(), Enumerate(dict), dict.Count, nestingLevel, ancestors, currentPath);
    }

    private string PrintDictionaryEntries(Type dictionaryType, IEnumerable<DictionaryEntry> entries, int? totalCount, int nestingLevel, HashSet<object> ancestors, MemberPath currentPath)
    {
        return PrintDictionaryCore(entries, dictionaryType, entries, totalCount, nestingLevel, ancestors, currentPath);
    }

    private string PrintDictionaryCore(object lockObj, Type dictType, IEnumerable<DictionaryEntry> entries, int? totalCount, int nestingLevel, HashSet<object> ancestors, MemberPath currentPath)
    {
        if (ancestors.Contains(lockObj))
            return "[cyclic reference]" + Environment.NewLine;

        ancestors.Add(lockObj);
        try
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var count = 0;

            foreach (var entry in entries)
            {
                if (count == 0)
                    sb.AppendLine(dictType.Name);

                if (count >= MaxSequenceElements)
                {
                    var countSuffix = totalCount.HasValue ? $", total count: {totalCount.Value}" : string.Empty;
                    sb.AppendLine($"{indentation}... [truncated{countSuffix}]");
                    break;
                }

                if (entry.Key is not null && IsTypeExcluded(entry.Key.GetType()))
                    continue;

                if (entry.Value is not null && IsTypeExcluded(entry.Value.GetType()))
                    continue;

                var keyStr = entry.Key is null
                    ? "null"
                    : FormatValue(entry.Key, currentPath, nestingLevel + 1, ancestors).TrimEnd();

                var valStr = entry.Value is null
                    ? "null" + Environment.NewLine
                    : FormatValue(entry.Value, currentPath, nestingLevel + 1, ancestors);

                sb.Append($"{indentation}[{keyStr}] = {valStr}");
                count++;
            }

            if (count == 0)
                return $"{dictType.Name} []" + Environment.NewLine;

            return sb.ToString();
        }
        finally
        {
            ancestors.Remove(lockObj);
        }
    }

    private static bool TryExtractDictionaryEntries(object value, out IEnumerable<DictionaryEntry> entries, out int? totalCount)
    {
        entries = null!;
        totalCount = null;

        var type = value.GetType();
        var genericDictInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || i.GetGenericTypeDefinition() == typeof(IDictionary<,>)));

        if (genericDictInterface is null)
            return false;

        var keyProp = genericDictInterface.GetGenericArguments()[0];
        var valProp = genericDictInterface.GetGenericArguments()[1];
        var kvpType = typeof(KeyValuePair<,>).MakeGenericType(keyProp, valProp);
        var getKey = kvpType.GetProperty("Key")!.GetMethod!;
        var getValue = kvpType.GetProperty("Value")!.GetMethod!;

        if (type.GetProperty("Count")?.GetValue(value) is int count)
            totalCount = count;

        entries = EnumerateKvp((IEnumerable)value, getKey, getValue);
        return true;

        static IEnumerable<DictionaryEntry> EnumerateKvp(IEnumerable seq, MethodInfo getKeyMethod, MethodInfo getValueMethod)
        {
            foreach (var item in seq)
            {
                if (item is null) continue;
                var k = getKeyMethod.Invoke(item, null);
                var v = getValueMethod.Invoke(item, null);
                yield return new DictionaryEntry(k!, v);
            }
        }
    }

    private string PrintEnumerable(IEnumerable sequence, int nestingLevel, HashSet<object> ancestors, MemberPath currentPath)
    {
        if (ancestors.Contains(sequence))
            return "[cyclic reference]" + Environment.NewLine;

        ancestors.Add(sequence);
        try
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var index = 0;

            foreach (var item in sequence)
            {
                if (index == 0)
                    sb.AppendLine(sequence.GetType().Name);

                if (index >= MaxSequenceElements)
                {
                    sb.AppendLine($"{indentation}... [truncated]");
                    break;
                }

                if (item is not null && IsTypeExcluded(item.GetType()))
                    continue;

                var itemStr = item is null
                    ? "null" + Environment.NewLine
                    : FormatValue(item, currentPath, nestingLevel + 1, ancestors);

                sb.Append($"{indentation}[{index}] = {itemStr}");
                index++;
            }

            if (index == 0)
                return $"{sequence.GetType().Name} []" + Environment.NewLine;

            return sb.ToString();
        }
        finally
        {
            ancestors.Remove(sequence);
        }
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
                {
                    sb.Append(indentation);
                    sb.Append(printedMember);
                }
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
        if (IsTypeExcluded(member.Type))
            return null;

        if (_rules.ExcludedMembers.Contains(memberPath))
            return null;

        object? value;
        try
        {
            value = member.GetValue(target);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            return $"{member.Name} = [throws {ex.InnerException?.GetType().Name ?? ex.GetType().Name}: {innerMessage}]{Environment.NewLine}";
        }

        if (value is not null && IsTypeExcluded(value.GetType()))
            return null;

        if (_rules.MemberSerializationRules.TryGetValue(memberPath, out var memberSerializer))
        {
            var serialized = memberSerializer(value);
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
            var seenNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var prop in t.GetProperties(flags))
            {
                if (prop.CanRead && prop.GetIndexParameters().Length == 0 && seenNames.Add(prop.Name))
                    list.Add(new MemberAccessor(prop.Name, prop.PropertyType, obj => prop.GetValue(obj)));
            }

            foreach (var field in t.GetFields(flags))
            {
                if (seenNames.Add(field.Name))
                    list.Add(new MemberAccessor(field.Name, field.FieldType, obj => field.GetValue(obj)));
            }

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

    private bool IsTypeExcluded(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        foreach (var excluded in _rules.ExcludedTypes)
        {
            var targetExcluded = Nullable.GetUnderlyingType(excluded) ?? excluded;
            if (targetExcluded == targetType || targetExcluded.IsAssignableFrom(targetType))
                return true;
        }

        return false;
    }

    private bool IsTerminal(Type type, MemberPath memberPath)
    {
        return type.IsPrimitive
            || type.IsEnum
            || TerminalTypes.Contains(type)
            || _rules.MemberCultureRules.ContainsKey(memberPath)
            || _rules.TypeCultureRules.ContainsKey(type);
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