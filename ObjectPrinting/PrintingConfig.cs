using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private HashSet<Type> defaultToStringTypes = new()
    {
        typeof(int),
        typeof(double),
        typeof(float),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan)
    };
    private string newline = Environment.NewLine;
    private const int EnumerableLimitCount = 9999;

    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<MemberInfo> excludedMembers = new();
    private readonly HashSet<object> serializedCache = new();

    private Dictionary<Type, Delegate> serializersForTypes = new();
    private Dictionary<MemberInfo, Delegate> serializersForMembers = new();

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this,
            serializersForTypes, serializersForMembers);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression)
            throw new ArgumentException("Incorrect member selector!");
        return new PropertyPrintingConfig<TOwner, TPropType>(this,
            serializersForTypes, serializersForMembers, memberSelector);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression)
            throw new ArgumentException("Incorrect member selector!");
        excludedMembers.Add(((MemberExpression)memberSelector.Body).Member);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel, MemberInfo info = null)
    {
        if (obj == null)
            return "null" + newline;

        if (defaultToStringTypes.Contains(obj.GetType()))
            return SerializeValue(obj, info);

        if (obj is IEnumerable enumerable)
            return SerializeEnumerable(enumerable, nestingLevel);

        return SerializeObject(obj, nestingLevel);
    }

    private string SerializeObject(object obj, int nestingLevel)
    {
        serializedCache.Add(obj);
        var identation = new string('\t', nestingLevel + 1);
        var builder = new StringBuilder();
        builder.AppendLine(obj.GetType().Name);

        foreach (var member in GetMembers(obj))
        {
            if (serializedCache.Contains(member.Value))
            {
                builder.Append($"{identation}{member.Info.Name} = [Loop reference]{newline}");
                continue;
            }

            SerializeMember(nestingLevel, identation, builder, member);
        }

        serializedCache.Remove(obj);
        return builder.ToString();
    }

    private string SerializeValue(object obj, MemberInfo info)
    {
        if (info != null && serializersForMembers.TryGetValue(info, out var memberSerializer))
            return memberSerializer.DynamicInvoke(obj) + newline;

        if (serializersForTypes.TryGetValue(obj.GetType(), out var typeSerializer))
            return typeSerializer.DynamicInvoke(obj) + newline;

        return obj + newline;
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
    {
        var builder = new StringBuilder(enumerable.GetType().Name + newline);
        var indentation = new string('\t', nestingLevel + 1);

        AppendBorders(nestingLevel, builder, true);

        var startLength = builder.Length;

        var enumerator = enumerable.GetEnumerator();
        var count = 0;

        while (enumerator.MoveNext() && count <= EnumerableLimitCount)
        {
            builder.Append($"{indentation}" +
                $"{PrintToString(enumerator.Current, nestingLevel + 1, null)}");
            count++;
        }

        if (builder.Length == startLength)
            builder.Append("Empty");

        AppendBorders(nestingLevel, builder, false);

        return builder.ToString();
    }

    private void AppendBorders(int nestingLevel, StringBuilder builder, bool isStart)
    {
        builder.Append(new string('\t', nestingLevel));
        var brackets = isStart ? "{{" : "}}";
        builder.Append(brackets);
        builder.Append(newline);
    }

    private IEnumerable<Member> GetMembers(object obj)
    {
        foreach (var member in obj.GetType()
            .GetMembers(BindingFlags.Instance | BindingFlags.Public))
        {
            if (excludedMembers.Contains(member))
                continue;

            if (member is FieldInfo field)
                if (!excludedTypes.Contains(field.FieldType))
                    yield return new Member(field, field.GetValue(obj));

            if (member is PropertyInfo property)
                if (!excludedTypes.Contains(property.PropertyType))
                    yield return new Member(property, property.GetValue(obj));
        }
    }

    private void SerializeMember(int nestingLevel, string identation,
        StringBuilder sb, Member member)
    {
        serializedCache.Add(member.Value);
        var serializedValue = $"{PrintToString(member.Value, nestingLevel + 1, member.Info)}";
        sb.Append($"{identation}{member.Info.Name} = {serializedValue}");
        serializedCache.Remove(member.Value);
    }
}