using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

/// <summary>
/// Сериализует объект в строку с настраиваемыми правилами:
/// исключением членов/типов, кастомными сериализаторами и культурой чисел
/// </summary>
public class ObjectSerializer
{
    private static readonly string NullRepresentation =
        "null" + Environment.NewLine;
    
    private readonly HashSet<Type> _excludedTypes = [];
    private readonly HashSet<MemberInfo> _excludedMembers = [];

    private readonly Dictionary<MemberInfo, Func<object?, string>>
        _modifiedMemberSerializers = [];

    private readonly Dictionary<Type, Func<object?, string>>
        _modifiedTypeSerializers = [];

    private readonly Dictionary<MemberInfo, CultureInfo> numericCultures = [];

    /// <summary>
    /// Сериализует <paramref name="obj"/> в строку.
    /// </summary>
    /// <param name="obj">Объект для сериализации</param>
    /// <returns>Строковое представление объекта с переводами строк и табуляцией</returns>
    public string SerializeObject(object? obj)
    {
        return PrintToString(obj, 0, 
            new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public void AddModifiedMemberSerializer<TMember>(MemberInfo memberInfo, Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(memberInfo);
        ArgumentNullException.ThrowIfNull(serializer);
        _modifiedMemberSerializers[memberInfo] = value => serializer((TMember)value!);
    }

    public void AddModifiedTypeSerializer<TMember>(Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        _modifiedTypeSerializers[typeof(TMember)] = value => serializer((TMember)value!);
    }

    public void AddExcludedType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _excludedTypes.Add(type);
    }

    public void AddExcludedMember(MemberInfo memberInfo)
    {
        ArgumentNullException.ThrowIfNull(memberInfo);
        _excludedMembers.Add(memberInfo);
    }

    public void SetNumericCulture(MemberInfo memberInfo, CultureInfo? cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(cultureInfo);
        numericCultures[memberInfo] = cultureInfo;
    }
    
    private string PrintToString(object? obj, int nestingLevel, HashSet<object> visited, MemberInfo? memberInfo = null)
    {
        if (obj == null)
            return NullRepresentation;

        if (_modifiedTypeSerializers.TryGetValue(
                obj.GetType(), out var typeSerializer))
            return typeSerializer(obj) + Environment.NewLine;
        
        if (obj.GetType().IsSimpleType())
            return SerializeSimpleTypeObject(obj, memberInfo);
        
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
            if (entry.Key.GetType().IsSimpleType()
                && entry.Value.GetType().IsSimpleType())
            {
                sb.Append(indentation)
                    .AppendLine($"[{entry.Key}] = {entry.Value}");
            }
            else {
                sb.Append(indentation)
                    .AppendLine($"[{index}]");

                sb.Append(entryIndentation)
                    .Append("Key = ")
                    .Append(PrintToString(entry.Key, nestingLevel + 2, visited));

                sb.Append(entryIndentation)
                    .Append("Value = ")
                    .Append(PrintToString(entry.Value, nestingLevel + 2, visited));
            }
            index++;
        }

        return sb.ToString();
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
    
    private string SerializeMembers(MemberInfo[] members, object obj, HashSet<object> visited, int nestingLevel)
    {
        var sb = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        foreach (var member in members)
        {
            if (member.IsSerializableMember())
                continue;

            var memberType = member.GetMemberType();

            if (_excludedMembers.Contains(member) || _excludedTypes.Contains(memberType))
                continue;

            var serializedValue = SerializeMember(obj, member, nestingLevel + 1, visited);

            sb.Append(indentation)
                .Append(member.Name)
                .Append(" = ")
                .Append(serializedValue);
        }

        return sb.ToString();
    }
    
    private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel, HashSet<object> visited)
    {
        var value = memberInfo.GetMemberValue(obj);

        if (value is null)
            return NullRepresentation;

        if (_modifiedMemberSerializers.TryGetValue(
                memberInfo, out var memberSerializer))
            return memberSerializer(value) + Environment.NewLine;

        return PrintToString(value, nestingLevel, visited, memberInfo);
    }
    
    private string SerializeSimpleTypeObject(object obj, MemberInfo? memberInfo = null)
    {
        var cultureInfo = memberInfo != null &&
                          numericCultures.TryGetValue(memberInfo, out var culture)
            ? culture
            : CultureInfo.CurrentCulture;
        
        var result = obj is IFormattable formattable
            ? formattable.ToString(null, cultureInfo)
            : obj.ToString();

        return result + Environment.NewLine;
    }
}