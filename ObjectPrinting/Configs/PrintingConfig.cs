using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Configs;

public class PrintingConfig<TOwner>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type[] FinalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    ];

    private readonly Dictionary<MemberInfo, Func<object, string>> memberFormatters = [];
    private readonly HashSet<MemberInfo> membersToExclude = [];

    private readonly Dictionary<Type, Func<object, string>> typeFormatters = [];
    private readonly HashSet<Type> typesToExclude = [];

    public PrintingConfig<TOwner> Excluding<TPropertyType>()
    {
        var propertyType = typeof(TPropertyType);
        typesToExclude.Add(propertyType);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropertyType>(
        Expression<Func<TOwner, TPropertyType>> memberSelector)
    {
        var member = GetSelectedMember(memberSelector);
        membersToExclude.Add(member);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropertyType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>(
        Expression<Func<TOwner, TPropertyType>> memberSelector)
    {
        var member = GetSelectedMember(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropertyType>(this, member);
    }

    internal void RegisterTypeFormatter<T>(Func<T, string> print)
    {
        typeFormatters[typeof(T)] = value => print((T)value);
    }

    internal void RegisterMemberFormatter<T>(MemberInfo member, Func<T, string> print)
    {
        memberFormatters[member] = value => print((T)value);
    }

    public string PrintToString(TOwner obj)
    {
        var currentPath = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return PrintToString(obj, 0, currentPath);
    }

    private string PrintToString(object? obj, int nestingLevel, HashSet<object> currentPath)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (typeFormatters.TryGetValue(type, out var formatter))
            return formatter(obj) + Environment.NewLine;

        if (FinalTypes.Contains(type))
            return obj + Environment.NewLine;

        if (!currentPath.Add(obj))
            return "<cycle>" + Environment.NewLine;

        try
        {
            if (obj is IEnumerable collection)
                return PrintCollection(collection, nestingLevel + 1, currentPath);

            return PrintObjectMembers(obj, type, nestingLevel + 1, currentPath);
        }
        finally
        {
            currentPath.Remove(obj);
        }
    }

    private string PrintObjectMembers(
        object obj, Type type, int memberLevel, HashSet<object> currentPath)
    {
        var indentation = new string('\t', memberLevel);
        var result = new StringBuilder().AppendLine(type.Name);

        foreach (var member in type.GetPrintableMembers())
        {
            if (typesToExclude.Contains(member.GetMemberType())
                || membersToExclude.Contains(member))
                continue;

            var value = member is PropertyInfo property
                ? property.GetValue(obj)
                : ((FieldInfo)member).GetValue(obj);

            var printedValue =
                value is not null && memberFormatters.TryGetValue(member, out var formatter)
                    ? formatter(value) + Environment.NewLine
                    : PrintToString(value, memberLevel, currentPath);

            result.Append(indentation)
                .Append(member.Name)
                .Append(" = ")
                .Append(printedValue);
        }

        return result.ToString();
    }

    private string PrintCollection(
        IEnumerable collection, int itemLevel, HashSet<object> currentPath)
    {
        var indentation = new string('\t', itemLevel);
        var result = new StringBuilder().AppendLine(collection.GetType().Name);
        var index = 0;

        if (collection is IDictionary dictionary)
            foreach (DictionaryEntry entry in dictionary)
                result.Append(indentation)
                    .Append('[').Append(entry.Key).Append("] = ")
                    .Append(PrintToString(entry.Value, itemLevel, currentPath));

        else
            foreach (var item in collection)
                result.Append(indentation)
                    .Append('[').Append(index++).Append("] = ")
                    .Append(PrintToString(item, itemLevel, currentPath));

        return result.ToString();
    }

    private static MemberInfo GetSelectedMember<TPropertyType>(Expression<Func<TOwner, TPropertyType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression memberExpression
            || memberExpression.Expression != memberSelector.Parameters[0]
            || memberExpression.Member is not (PropertyInfo or FieldInfo))
            throw new ArgumentException("Выражение должно указывать на свойство или поле.",
                nameof(memberSelector));

        return memberExpression.Member;
    }
}