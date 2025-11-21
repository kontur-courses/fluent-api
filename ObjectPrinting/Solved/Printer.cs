using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved;

public class Printer(PrintingSettings settings)
{
    private readonly HashSet<object?> parsedObjects = [];

    public string PrintToString(object? obj, int nestingLevel, MemberInfo? member)
    {
        var sb = new StringBuilder();
        if (TryPrintNullOrExcluded(obj, nestingLevel, sb))
            return sb.ToString();

        var type = obj!.GetType();

        if (TryPrintSimpleType(obj, type, member, sb))
            return sb.ToString();

        var isRemoveNeeded = false;
        
        if (!type.IsValueType && obj is not string)
        {
            if (!parsedObjects.Add(obj))
                return $"Cyclic reference at {type.Name}" + Environment.NewLine;
            isRemoveNeeded = true;
        }

        var indentation = new string('\t', nestingLevel + 1);
        var result = string.Empty;
        
        if (obj is IDictionary dictionary)
            result = ProcessDictionary(dictionary, type, indentation, nestingLevel);
        else if (obj is IEnumerable enumerable and not string)
            result = ProcessEnumerable(enumerable, type, indentation, nestingLevel);
        else
            result = PrintComplexType(obj, type, indentation, nestingLevel);
        
        if (isRemoveNeeded)
            parsedObjects.Remove(obj);
        
        return result;
    }

    private string PrintComplexType(object obj, Type type, string indentation, int nestingLevel)
    {
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        var printableMembers = GetPrintableMembers(type);
        foreach (var memberInfo in printableMembers)
        {
            var value = GetMemberValue(memberInfo, obj);
            sb.Append($"{indentation}{memberInfo.Name} = {PrintToString(value, nestingLevel + 1, memberInfo)}");
        }

        return sb.ToString();
    }

    private bool TryPrintNullOrExcluded(object? obj, int nestingLevel, StringBuilder result)
    {
        if (obj is null)
        {
            result.Append("null" + Environment.NewLine);
            return true;
        }

        var type = obj.GetType();

        if (settings.ExcludedTypes.Contains(type) && nestingLevel > 0)
            return true;

        return false;
    }

    private string ProcessDictionary(IDictionary dictionary, Type type, string indentation, int nestingLevel)
    {
        var result = new StringBuilder();
        result.AppendLine(type.Name);
        foreach (DictionaryEntry entry in dictionary)
        {
            result.Append($"{indentation}Key = {PrintToString(entry.Key, nestingLevel + 1, null)}");
            result.Append($"{indentation}Value = {PrintToString(entry.Value, nestingLevel + 1, null)}");
        }
        return result.ToString();
    }

    private string ProcessEnumerable(IEnumerable enumerable, Type type, string indentation, int nestingLevel)
    {
        var result = new StringBuilder();
        
        result.AppendLine(type.Name);
        var index = 0;
        foreach (var item in enumerable)
        {
            result.Append($"{indentation}[{index}] = {PrintToString(item, nestingLevel + 1, null)}");
            index++;
        }
        
        return result.ToString();
    }

    private bool TryPrintSimpleType(object obj, Type type, MemberInfo? memberInfo, StringBuilder result)
    {
        if (memberInfo != null && settings.MemberSerializers.TryGetValue(memberInfo, out var memberSerializer))
        {
            result.Append(memberSerializer(obj) + Environment.NewLine);
            return true;
        }

        if (settings.TypeSerializers.TryGetValue(type, out var typeSerializer))
        {
            result.Append(typeSerializer(obj) + Environment.NewLine);
            return true;
        }

        if (memberInfo != null && settings.MemberCultures.TryGetValue(memberInfo, out var memberCulture) &&
            obj is IFormattable formattable1)
        {
            result.Append(formattable1.ToString(null, memberCulture) + Environment.NewLine);
            return true;
        }

        if (settings.TypeCultures.TryGetValue(type, out var typeCulture) && obj is IFormattable formattable2)
        {
            result.Append(formattable2.ToString(null, typeCulture) + Environment.NewLine);
            return true;
        }

        if (settings.PrimitiveTypes.Contains(type) || type.IsEnum)
        {
            result.Append(obj + Environment.NewLine);
            return true;
        }

        return false;
    }

    private List<MemberInfo> GetPrintableMembers(Type type)
    {
        var result = new List<MemberInfo>();
        var flags = BindingFlags.Instance | BindingFlags.Public;
        foreach (var propertyInfo in type.GetProperties(flags))
        {
            if (!propertyInfo.CanRead)
                continue;

            if (IsMemberExcluded(propertyInfo, propertyInfo.PropertyType))
                continue;

            result.Add(propertyInfo);
        }

        foreach (var fieldInfo in type.GetFields(flags))
        {
            if (IsMemberExcluded(fieldInfo, fieldInfo.FieldType))
                continue;

            result.Add(fieldInfo);
        }

        return result;
    }

    private bool IsMemberExcluded(MemberInfo member, Type memberType)
    {
        return settings.ExcludedMembers.Contains(member) || settings.ExcludedTypes.Contains(memberType);
    }

    private object? GetMemberValue(MemberInfo member, object obj)
    {
        return member switch
        {
            PropertyInfo p => p.GetValue(obj),
            FieldInfo f => f.GetValue(obj),
            _ => null
        };
    }
}