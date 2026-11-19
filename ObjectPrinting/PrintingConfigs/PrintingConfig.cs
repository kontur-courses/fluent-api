using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObjectPrinting.PrintingConfigs;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = new ();
    private readonly HashSet<(Type, string)> excludedMembers = new ();
    private readonly Dictionary<Type, Delegate> typeSerializers = new ();
    private readonly Dictionary<(Type, string), Delegate> memberSerializers = new ();

    private static readonly Type[] finalTypes =
    {
        typeof(int), typeof(long), typeof(double), typeof(float), typeof(decimal),
        typeof(string), typeof(bool), typeof(char), typeof(Guid),
        typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)
    };

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMember(memberSelector));
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        excludedMembers.Add(Key(GetMember(memberSelector)));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    internal void SetTypeSerializer(Type type, Delegate serializer)
    {
        typeSerializers[type] = serializer;
    }

    internal void SetMemberSerializer(MemberInfo member, Delegate serializer)
    {
        memberSerializers[Key(member)] = serializer;
    }

    private static MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> selector)
    {
        if (selector.Body is MemberExpression memberExpression)
            return memberExpression.Member;
        throw new ArgumentException("Expression must select a property or field", nameof(selector));
    }

    private static (Type, string) Key(MemberInfo member) => (member.DeclaringType, member.Name);
        
    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0, new HashSet<object>(new ReferenceComparer()));
    }

    private string PrintToString(object? obj, int nestingLevel, HashSet<object> path)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (typeSerializers.TryGetValue(type, out var typeSerializer))
            return (string)typeSerializer.DynamicInvoke(obj) + Environment.NewLine;

        if (IsFinalType(type))
            return obj + Environment.NewLine;
            
        var isReference = !type.IsValueType;
        if (isReference && !path.Add(obj))
            return "cyclic reference to " + GetTypeName(type) + Environment.NewLine;

        var result = obj is IEnumerable collection
            ? PrintCollection(collection, type, nestingLevel, path)
            : PrintObject(obj, type, nestingLevel, path);
            
        if (isReference)
            path.Remove(obj);

        return result;
    }

    private string PrintObject(object obj, Type type, int nestingLevel, HashSet<object> path)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(GetTypeName(type));

        foreach (var property in type.GetProperties())
        {
            if (property.GetIndexParameters().Length > 0 ||
                excludedTypes.Contains(property.PropertyType) ||
                excludedMembers.Contains(Key(property)))
                continue;

            sb.Append(indentation + property.Name + " = " +
                      PrintMember(property, obj, nestingLevel, path));
        }

        return sb.ToString();
    }

    private string PrintMember(PropertyInfo property, object owner, int nestingLevel, HashSet<object> path)
    {
        var value = property.GetValue(owner);
            
        if (value != null && memberSerializers.TryGetValue(Key(property), out var serializer))
            return (string)serializer.DynamicInvoke(value) + Environment.NewLine;

        return PrintToString(value, nestingLevel + 1, path);
    }

    private string PrintCollection(IEnumerable collection, Type type, int nestingLevel, HashSet<object> path)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(GetTypeName(type));

        if (collection is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                var key = PrintToString(entry.Key, nestingLevel + 1, path).TrimEnd('\r', '\n');
                var value = PrintToString(entry.Value, nestingLevel + 1, path);
                sb.Append($"{indentation}[{key}] = {value}");
            }
        }
        else
        {
            var index = 0;
            foreach (var item in collection)
                sb.Append($"{indentation}[{index++}] = {PrintToString(item, nestingLevel + 1, path)}");
        }

        return sb.ToString();
    }

    private static bool IsFinalType(Type type) => type.IsEnum || finalTypes.Contains(type);
        
    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var name = type.Name.Substring(0, type.Name.IndexOf('`'));
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(GetTypeName))}>";
    }

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}