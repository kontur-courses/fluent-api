using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

internal class ObjectSerializer
{
    private readonly IReadOnlyDictionary<Type, PrintingRule> _typeRules;
    private readonly IReadOnlyDictionary<MemberInfo, PrintingRule> _memberRules;

    private readonly HashSet<object> _ancestors =
        new(ReferenceEqualityComparer.Instance);

    private readonly StringBuilder _output = new();

    internal ObjectSerializer(
        IReadOnlyDictionary<Type, PrintingRule> typeRules,
        IReadOnlyDictionary<MemberInfo, PrintingRule> memberRules)
    {
        _typeRules = typeRules;
        _memberRules = memberRules;
    }

    internal string Serialize(object? value, Type declaredType)
    {
        var rule = FindTypeRule(declaredType, value);

        if (rule?.IsExcluded != true)
            WriteValue(value, 0, rule);

        return _output.ToString();
    }

    private void WriteValue(
        object? value,
        int depth,
        PrintingRule? typeRule,
        PrintingRule? memberRule = null)
    {
        if (value is null)
        {
            _output.AppendLine("null");
            return;
        }

        var serializer = memberRule?.Serializer ?? typeRule?.Serializer;
        if (serializer is not null)
        {
            _output.AppendLine(serializer(value));
            return;
        }

        var culture = memberRule?.Culture ?? typeRule?.Culture;
        if (culture is not null && value is IFormattable formattable)
        {
            _output.AppendLine(formattable.ToString(null, culture));
            return;
        }

        if (IsSimple(value))
        {
            _output.AppendLine(value.ToString());
            return;
        }

        if (!_ancestors.Add(value))
        {
            _output.AppendLine("[cycle]");
            return;
        }

        try
        {
            _output.AppendLine(value.GetType().Name);

            if (value is IEnumerable collection)
                WriteCollection(collection, depth);
            else
                WriteMembers(value, depth);
        }
        finally
        {
            _ancestors.Remove(value);
        }
    }

    private void WriteMembers(object owner, int depth)
    {
        foreach (var member in PrintableMember.GetAll(owner.GetType()))
        {
            _memberRules.TryGetValue(member.Key, out var memberRule);

            var declaredRule = FindTypeRule(member.ValueType);
            if (memberRule?.IsExcluded == true ||
                declaredRule?.IsExcluded == true)
            {
                continue;
            }

            var value = member.GetValue(owner);
            var typeRule = declaredRule ??
                           FindTypeRule(member.ValueType, value);

            if (typeRule?.IsExcluded == true)
                continue;

            WriteNamedValue(member.Name, value, depth + 1, typeRule, memberRule);
        }
    }

    private void WriteCollection(IEnumerable collection, int depth)
    {
        var index = 0;

        foreach (var item in collection)
        {
            var typeRule = item is null
                ? null
                : FindTypeRule(item.GetType());

            if (typeRule?.IsExcluded != true)
                WriteNamedValue($"[{index}]", item, depth + 1, typeRule);

            index++;
        }
    }

    private void WriteNamedValue(
        string name,
        object? value,
        int depth,
        PrintingRule? typeRule,
        PrintingRule? memberRule = null)
    {
        _output.Append('\t', depth)
            .Append(name)
            .Append(" = ");

        WriteValue(value, depth, typeRule, memberRule);
    }

    private PrintingRule? FindTypeRule(
        Type declaredType, object? value = null)
    {
        if (_typeRules.TryGetValue(declaredType, out var rule))
            return rule;

        if (value is not null &&
            _typeRules.TryGetValue(value.GetType(), out rule))
        {
            return rule;
        }

        return null;
    }

    private static bool IsSimple(object value)
    {
        var type = value.GetType();

        return type.IsPrimitive ||
               type.IsEnum ||
               value is string or decimal or Guid or
                   DateTime or DateTimeOffset or TimeSpan or
                   DateOnly or TimeOnly;
    }
}