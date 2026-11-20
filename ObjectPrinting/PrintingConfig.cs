using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<MemberInfo> excludedMembers = [];

    private readonly Dictionary<MemberInfo, Func<object?, string>>
        modifiedMemberSerializers = [];

    private readonly Dictionary<Type, Func<object?, string>>
        modifiedTypeSerializers = [];

    private CultureInfo cultureInfo = CultureInfo.CurrentCulture;

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0, 
            new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public PrintingConfig<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        excludedMembers.Add(GetMemberInfo(selector));
        return this;
    }

    public PrintingConfig<TOwner> SetModifiedSerialization<TMember>
        (Expression<Func<TOwner, TMember>> memberSelector, Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        var memberInfo = GetMemberInfo(memberSelector);
        modifiedMemberSerializers[memberInfo] = value => serializer((TMember)value!);
        return this;
    }

    public PrintingConfig<TOwner> SetTypeSerialization<TMember>(Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        modifiedTypeSerializers[typeof(TMember)] = value => serializer((TMember)value!);
        return this;
    }

    public PrintingConfig<TOwner> SetNumericCulture(CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(cultureInfo);

        this.cultureInfo = cultureInfo;
        return this;
    }

    public PrintingConfig<TOwner> Truncate(Expression<Func<TOwner, string>> selector, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        return SetModifiedSerialization(
            selector,
            value => value[..Math.Min(length, value.Length)]);
    }

    private static MemberInfo GetMemberInfo<TMember>(
        Expression<Func<TOwner, TMember>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var body = selector.Body;

        if (body is UnaryExpression unary &&
            unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;

        if (body is not MemberExpression memberExpr ||
            memberExpr.Member is not (PropertyInfo or FieldInfo) ||
            memberExpr.Expression is not ParameterExpression)
        {
            throw new ArgumentException(
                "Ожидается прямое обращение к полю или свойству объекта",
                nameof(selector));
        }

        return memberExpr.Member;
    }

    private string SerializeMembers(MemberInfo[] members, object obj, HashSet<object> visited, int nestingLevel)
    {
        var sb = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        foreach (var member in members)
        {
            if (member is PropertyInfo property &&
                (property.GetMethod?.IsPublic != true ||
                 property.GetIndexParameters().Length > 0))
                continue;

            var memberType = GetMemberType(member);

            if (excludedMembers.Contains(member) || excludedTypes.Contains(memberType))
                continue;

            var serializedValue = SerializeMember(obj, member, nestingLevel + 1, visited);

            sb.Append(indentation)
                .Append(member.Name)
                .Append(" = ")
                .Append(serializedValue);
        }

        return sb.ToString();
    }

    private string SerializeSimpleTypeObject(object obj)
    {
        var result = obj is IFormattable formattable
            ? formattable.ToString(null, cultureInfo)
            : obj.ToString();

        return result + Environment.NewLine;
    }

    private string SerializeComplexTypeObject(object obj, HashSet<object> visited, int nestingLevel)
    {
        var sb = new StringBuilder();
        var type = obj.GetType();
        var members = type.FindMembers(
            MemberTypes.Property | MemberTypes.Field,
            BindingFlags.Public | BindingFlags.Instance,
            null,
            null);
        sb.AppendLine(type.Name);
        sb.Append(SerializeMembers(members, obj, visited, nestingLevel));

        return sb.ToString();
    }
    
    private string PrintToString(object? obj, int nestingLevel, HashSet<object> visited)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        if (modifiedTypeSerializers.TryGetValue(
                obj.GetType(), out var typeSerializer))
            return typeSerializer(obj) + Environment.NewLine;
        
        if (IsSimpleType(obj.GetType()))
            return SerializeSimpleTypeObject(obj);
        
        if (!visited.Add(obj))
            return $"{obj.GetType().Name} - cycle link" + Environment.NewLine;

        try
        {
            return obj switch
            {
                IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel, visited),
                IEnumerable enumerable => SerializeCollection(enumerable, nestingLevel, visited),
                _ => SerializeComplexTypeObject(obj, visited, nestingLevel)
            };
        }
        finally
        {
            visited.Remove(obj);
        }
    }

    private string SerializeCollection(IEnumerable enumerable, int nestingLevel, HashSet<object> visited)
    {
        var sb = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);

        sb.AppendLine(enumerable.GetType().Name);

        var index = 0;

        foreach (var item in enumerable)
        {
            sb.Append(indentation)
                .Append($"[{index}] = ")
                .Append(PrintToString(item, nestingLevel + 1, visited));

            index++;
        }

        return sb.ToString();
    }

    private string SerializeDictionary(
        IDictionary dictionary,
        int nestingLevel,
        HashSet<object> visited)
    {
        var sb = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        var entryIndentation = new string('\t', nestingLevel + 2);

        sb.AppendLine(dictionary.GetType().Name);

        var index = 0;

        foreach (DictionaryEntry entry in dictionary)
        {
            sb.Append(indentation)
                .AppendLine($"[{index}]");

            sb.Append(entryIndentation)
                .Append("Key = ")
                .Append(PrintToString(entry.Key, nestingLevel + 2, visited));

            sb.Append(entryIndentation)
                .Append("Value = ")
                .Append(PrintToString(entry.Value, nestingLevel + 2, visited));

            index++;
        }

        return sb.ToString();
    }

    private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel, HashSet<object> visited)
    {
        var value = GetMemberValue(memberInfo, obj);

        if (value is null)
            return "null" + Environment.NewLine;

        if (modifiedMemberSerializers.TryGetValue(
                memberInfo, out var memberSerializer))
            return memberSerializer(value) + Environment.NewLine;

        return PrintToString(value, nestingLevel, visited);
    }

    private static Type GetMemberType(MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new NotSupportedException()
        };
    }

    private static object? GetMemberValue(MemberInfo memberInfo, object obj)
    {
        return memberInfo switch
        {
            PropertyInfo property => property.GetValue(obj),
            FieldInfo field => field.GetValue(obj),
            _ => throw new NotSupportedException()
        };
    }
    
    private static bool IsSimpleType(Type type)
    {
        // наверно, есть способ лучше
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(Guid)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(DateOnly)
               || type == typeof(TimeOnly);
    }
}