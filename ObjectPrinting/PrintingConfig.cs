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
    private readonly List<Type> excludedTypes = new();
    private readonly List<MemberInfo> excludedProperties = new();
    private readonly Dictionary<Type, Delegate> serializedByType = new();

    private readonly Dictionary<MemberInfo, Delegate> serializedByMemberInfo =
        new();

    private readonly HashSet<object> openObjects = new();

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    public PropertyPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
    {
        return new PropertyPrintingConfig<TOwner, TMemberType>(this, null);
    }

    public PropertyPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TMemberType>(this, GetMemberInfo(memberSelector));
    }

    public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        excludedProperties.Add(GetMemberInfo(memberSelector));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TMemberType>()
    {
        excludedTypes.Add(typeof(TMemberType));
        return this;
    }

    private static MemberInfo GetMemberInfo<TOwner, TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"Expression '{memberSelector}' refers to a method, not a property or field.");

        if (!IsPropertyOrFieldMember(memberExpression.Member))
            throw new ArgumentException($"Expression '{memberSelector}' refers to an unsupported member type.");
        
        return memberExpression.Member;
    }

    private static bool IsFinalType(Type type)
    {
        return type == typeof(string)
               || type.IsPrimitive
               || typeof(IFormattable).IsAssignableFrom(type);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();
        if (IsFinalType(type))
            return obj + Environment.NewLine;

        var sb = new StringBuilder();
        if (openObjects.Any(x => ReferenceEquals(x, obj)))
            return sb.AppendLine("(Cycle)" + type.FullName).ToString();
        openObjects.Add(obj);
        sb.AppendLine(type.Name);
        if (IsDictionary(type)) return sb.Append(SerializeEnumerable(obj, nestingLevel)).ToString();
        
        if (IsArrayOrList(type)) return sb.Append(SerializeEnumerable(obj, nestingLevel)).ToString();

        sb.Append(PrintPropertiesAndFields(obj, nestingLevel, type));
        openObjects.Remove(obj);
        return sb.ToString();
    }

    private static Type GetType(MemberInfo member)
    {
        if (!IsPropertyOrFieldMember(member))
            throw new ArgumentException($"Expression '{member}' refers to a method, not a property or field.");
        return (member as FieldInfo)?.FieldType
               ?? (member as PropertyInfo)?.PropertyType;
    }

    private static object GetValue(MemberInfo member, object obj)
    {
        if (!IsPropertyOrFieldMember(member))
            throw new ArgumentException($"Expression '{member}' refers to a method, not a property or field.");
        return (member as FieldInfo)?.GetValue(obj)
               ?? (member as PropertyInfo)?.GetValue(obj);
    }
    private string PrintPropertiesAndFields(object obj, int nestingLevel, Type type)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        foreach (var memberInfo in type.GetMembers().Where(prop =>IsPropertyOrFieldMember(prop) && !IsExcluded(prop)))
        {
            if (TrySerializeMember(obj, memberInfo, GetType(memberInfo), out var serializedValue))
            {
                sb.AppendLine($"{indentation}{memberInfo.Name} = {serializedValue}");
                continue;
            }

            if (IsDictionary(GetType(memberInfo)))
            {
                sb.AppendLine(indentation + memberInfo.Name + " = ");
                sb.Append(SerializeDictionary(GetValue(memberInfo, obj), nestingLevel + 1));
            }
            else if (IsArrayOrList(GetType(memberInfo)))
            {
                sb.AppendLine(indentation + memberInfo.Name + " = ");
                sb.Append(SerializeEnumerable(GetValue(memberInfo, obj), nestingLevel + 1));
            }
            else
            {
                sb.Append(
                    $"{indentation}{memberInfo.Name} = " +
                    $"{PrintToString(GetValue(memberInfo, obj), nestingLevel + 1)}"
                );
            }
        }


        return sb.ToString();
    }

    private static bool IsPropertyOrFieldMember(MemberInfo member)
    {
        return member.MemberType is MemberTypes.Field or MemberTypes.Property;
    }
    private bool TrySerializeMember(object obj, MemberInfo memberInfo, Type propertyType,
        out string serializedValue)
    {
        serializedValue = null;
        Delegate valueToUse = null;
        if (serializedByMemberInfo.TryGetValue(memberInfo, out var propertyValue))
            valueToUse = propertyValue;
        if (serializedByType.TryGetValue(propertyType, out var typeValue))
            valueToUse = typeValue;

        return valueToUse != null && TrySerializeValue(valueToUse, GetValue(memberInfo, obj), out serializedValue);
    }

    private static bool TrySerializeValue(Delegate serializer, object value, out string serializedValue)
    {
        try
        {
            serializedValue = serializer.DynamicInvoke(value)?.ToString();
            return true;
        }
        catch
        {
            serializedValue = null;
            return false;
        }
    }

    private static bool IsArrayOrList(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) && !IsFinalType(type);
    }

    private static bool IsDictionary(Type type)
    {
        return typeof(IDictionary).IsAssignableFrom(type);
    }

    private string SerializeEnumerable(object obj, int nestingLevel)
    {
        var enumerable = (IEnumerable)obj;
        var sb = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        if (enumerable == null) return $"{indentation}null" + Environment.NewLine;
        foreach (var value in enumerable)
        {
            sb.Append(indentation);
            sb.Append(PrintToString(value, nestingLevel + 1));
        }

        return sb.ToString();
    }

    private string SerializeDictionary(object obj, int nestingLevel)
    {
        var dictionary = (IDictionary)obj;
        var sb = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        if (dictionary == null) return $"{indentation}null" + Environment.NewLine;
        foreach (var keyVal in dictionary)
        {
            sb.Append(indentation);
            var key = ((DictionaryEntry)keyVal).Key;
            var value = ((DictionaryEntry)keyVal).Value;
            sb.Append(PrintToString(key, nestingLevel) + " : " + PrintToString(value, nestingLevel + 1));
        }

        return sb.ToString();
    }

    private bool IsExcluded(MemberInfo memberInfo)
    {
        return excludedProperties.Contains(memberInfo) || excludedTypes.Contains(GetType(memberInfo));
    }

    public void AddSerializeMember<TMemberType>(Func<TMemberType, string> print, MemberInfo memberInfo)
    {
        serializedByMemberInfo.Add(memberInfo, print);
    }

    public void AddSerializeByType<TMemberType>(Type type, Func<TMemberType, string> print)
    {
        serializedByType.Add(type, print);
    }
}