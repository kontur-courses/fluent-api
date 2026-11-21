using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private const int MaxNestingLevel = 20;

    private static readonly HashSet<Type> FinalTypes =
    [
        typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid)
    ];

    private readonly HashSet<Type> excludedTypes = [];

    private readonly HashSet<string> excludedMembers = [];

    private readonly Dictionary<Type, Func<object, string>> typeSerializers = [];

    private readonly Dictionary<string, Func<object, string>> memberSerializers = [];

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberPath(memberSelector));
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        excludedMembers.Add(GetMemberPath(memberSelector));

        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));

        return this;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0, string.Empty, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public void AddTypeSerializer(Type type, Func<object, string> serializer)
    {
        typeSerializers[type] = serializer;
    }

    public void AddMemberSerializer(string memberPath, Func<object, string> serializer)
    {
        memberSerializers[memberPath] = serializer;
    }

    private string PrintToString(object? obj, int nestingLevel, string path, HashSet<object> visited)
    {
        if (obj is null)
        {
            return "null";
        }

        if (memberSerializers.TryGetValue(path, out var memberSerializer))
        {
            return memberSerializer(obj);
        }

        var type = obj.GetType();
        if (typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            return typeSerializer(obj);
        }

        if (type.IsPrimitive || type.IsEnum || FinalTypes.Contains(type))
        {
            return obj.ToString()!;
        }

        if (nestingLevel > MaxNestingLevel)
        {
            return "<max nesting level reached>";
        }

        if (!visited.Add(obj))
        {
            return "<cyclic reference>";
        }

        var result = string.Empty;
        switch (obj)
        {
            case IDictionary dictionary:
                result = PrintDictionary(dictionary, nestingLevel, path, visited);
                break;
            case IEnumerable collection:
                result = PrintCollection(collection, nestingLevel, path, visited);
                break;
            default:
                result = PrintMembers(obj, nestingLevel, path, visited);
                break;
        }

        visited.Remove(obj);
        return result;
    }

    private string PrintMembers(object obj, int nestingLevel, string path, HashSet<object> visited)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var stringBuilder = new StringBuilder();
        var type = obj.GetType();
        
        stringBuilder.Append(type.Name);
        foreach (var member in GetMembers(type))
        {
            var memberPath = path == string.Empty 
                ? member.Name 
                : $"{path}.{member.Name}";
            if (excludedTypes.Contains(GetMemberType(member)) || excludedMembers.Contains(memberPath))
            {
                continue;
            }

            var value = PrintToString(GetMemberValue(member, obj), nestingLevel + 1, memberPath, visited);
            stringBuilder.Append(Environment.NewLine + indentation + member.Name + " = " + value);
        }

        return stringBuilder.ToString();
    }

    private string PrintCollection(IEnumerable collection, int nestingLevel, string path, HashSet<object> visited)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var stringBuilder = new StringBuilder("[");
        foreach (var item in collection)
        {
            var value = PrintToString(item, nestingLevel + 1, path + "[]", visited);
            stringBuilder.Append(Environment.NewLine + indentation + value);
        }

        stringBuilder.Append(Environment.NewLine + new string('\t', nestingLevel) + "]");
        return stringBuilder.ToString();
    }

    private string PrintDictionary(IDictionary dictionary, int nestingLevel, string path, HashSet<object> visited)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var stringBuilder = new StringBuilder("{");
        foreach (DictionaryEntry entry in dictionary)
        {
            var key = PrintToString(entry.Key, nestingLevel + 1, path + "[]", visited);
            var value = PrintToString(entry.Value, nestingLevel + 1, path + "[]", visited);
            stringBuilder.Append(Environment.NewLine + indentation + key + ": " + value);
        }

        stringBuilder.Append(Environment.NewLine + new string('\t', nestingLevel) + "}");
        return stringBuilder.ToString();
    }

    private static IEnumerable<MemberInfo> GetMembers(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo => propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        return properties.Concat<MemberInfo>(fields);
    }

    private static Type GetMemberType(MemberInfo member)
    {
        return member is PropertyInfo property 
            ? property.PropertyType 
            : ((FieldInfo)member).FieldType;
    }

    private static object? GetMemberValue(MemberInfo member, object obj)
    {
        return member is PropertyInfo property 
            ? property.GetValue(obj) 
            : ((FieldInfo)member).GetValue(obj);
    }

    private static string GetMemberPath<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var names = new List<string>();
        var expression = memberSelector.Body;
        while (expression is MemberExpression memberExpression)
        {
            names.Add(memberExpression.Member.Name);
            expression = memberExpression.Expression;
        }

        if (expression is not ParameterExpression || names.Count == 0)
        {
            throw new ArgumentException(
                $"Expression must be a chain of properties or fields: {memberSelector}", nameof(memberSelector));
        }

        names.Reverse();
        return string.Join(".", names);
    }
}