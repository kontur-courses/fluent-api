using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.PrintingConfigs;

namespace ObjectPrinting;

public class ObjectPrinter
{
    private readonly PrinterSettings settings;

    public ObjectPrinter(PrinterSettings settings)
    {
        this.settings = settings;
    }

    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }
        
    public string PrintToString<TOwner>(TOwner obj)
    {
        return PrintToString(obj, 0, new HashSet<object>());
    }

    private string PrintToString(object? obj, int nestingLevel, HashSet<object> visited)
    {
        if (nestingLevel > settings.MaxRecursionDepth)
            return string.Empty;
        
        if (obj is null)
            return "null" + NewLine;

        var type = obj.GetType();

        if (settings.ExcludedTypes.Contains(type))
            return string.Empty;

        if (IsCyclicReference(obj, type, visited))
            return "(cyclic reference)" + NewLine;

        return FormatByType(obj, nestingLevel, visited, type);
    }
    
    private string FormatByType(object obj, int nestingLevel, HashSet<object> visited, Type type)
    {
        if (TrySerializeType(type, obj, out var result))
            return result + NewLine;
    
        if (TryFormatFormattable(obj, type, out result))
            return result + NewLine;
    
        if (IsFinalType(type))
            return Convert.ToString(obj, CultureInfo.InvariantCulture) + NewLine;

        return obj switch
        {
            IDictionary dictionary => PrintDictionary(dictionary, nestingLevel, visited),
            IEnumerable enumerable => PrintEnumerable(enumerable, nestingLevel, visited),
            _ => PrintObject(obj, nestingLevel, visited)
        };
    }
    
    private string PrintObject(object obj, int nestingLevel, HashSet<object> visited)
    {
        var type = obj.GetType();
        var indent = Indent(nestingLevel + 1);
        var sb = new StringBuilder();

        sb.AppendLine(type.Name);

        var members = GetFilteredMembers(type);

        foreach (var member in members)
        {
            sb.Append(ProcessMember(obj, member, indent, nestingLevel, visited));
        }

        return sb.ToString();
    }
    
    private string ProcessMember(
        object obj, MemberInfo member, string indent, int nestingLevel, HashSet<object> visited)
    {
        var value = GetMemberValue(obj, member);
        var prefix = $"{indent}{member.Name} = ";

        if (value is null)
            return prefix + "null" + NewLine;
        
        if (settings.ExcludedTypes.Contains(value.GetType()))
            return string.Empty;

        if (TrySerializeMember(member, value, out var result))
            return prefix + result + NewLine;
        
        if (TryTrimStringMember(member, value, out var trimmed))
            return prefix + trimmed + NewLine;

        return prefix + PrintToString(value, nestingLevel + 1, visited);
    }

    private string PrintEnumerable(IEnumerable enumerable, int nestingLevel, HashSet<object> visited)
    {
        var indent = Indent(nestingLevel + 1);
        var sb = new StringBuilder();

        sb.AppendLine("[");
        
        foreach (var item in enumerable)
        {
            sb.Append(indent).Append(PrintToString(item, nestingLevel + 1, visited));
        }

        sb.Append(Indent(nestingLevel)).AppendLine("]");
        return sb.ToString();
    }
    
    private string PrintDictionary(IDictionary dict, int nestingLevel, HashSet<object> visited)
    {
        var indent = Indent(nestingLevel + 1);
        var sb = new StringBuilder();

        sb.AppendLine("{");

        foreach (DictionaryEntry entry in dict)
        {
            sb.Append(indent)
                .Append('[')
                .Append(PrintToString(entry.Key, nestingLevel + 1, visited).TrimEnd())
                .Append("] = ")
                .Append(PrintToString(entry.Value, nestingLevel + 1, visited));
        }

        sb.Append(Indent(nestingLevel)).AppendLine("}");
        return sb.ToString();
    }
    
    private bool TrySerializeType(Type type, object obj, out string? result)
    {
        if (settings.TypeSerializers.TryGetValue(type, out var serializer))
        {
            result = serializer(obj);
            return true;
        }
    
        result = null;
        return false;
    }
    
    private bool TrySerializeMember(MemberInfo member, object value, out string? result)
    {
        if (settings.MemberSerializers.TryGetValue(member, out var serializer))
        {
            result = Convert.ToString(serializer(value), CultureInfo.InvariantCulture);
            return true;
        }
    
        result = null;
        return false;
    }

    private bool TryFormatFormattable(object obj, Type type, out string? result)
    {
        if (obj is IFormattable formattable && settings.TypeCultures.TryGetValue(type, out var culture))
        {
            result = Convert.ToString(formattable, culture);
            return true;
        }
    
        result = string.Empty;
        return false;
    }
    
    private bool TryTrimStringMember(MemberInfo member, object value, out string? result)
    {
        if (value is string stringValue && settings.TrimLengths.TryGetValue(member, out var trimLength))
        {
            result = stringValue.Length > trimLength 
                ? stringValue.Substring(0, trimLength) 
                : stringValue;
            return true;
        }
    
        result = null;
        return false;
    }
    
    private static bool IsCyclicReference(object obj, Type type, HashSet<object> visited)
    {
        return !type.IsValueType && !visited.Add(obj);
    }
    
    private static bool IsFinalType(Type type)
    {
        return type.IsPrimitive
               || type == typeof(string)
               || type.IsEnum
               || typeof(IFormattable).IsAssignableFrom(type);
    }
    
    private IEnumerable<MemberInfo> GetFilteredMembers(Type type)
    {
        return type.GetProperties()
            .Concat(type.GetFields().Cast<MemberInfo>())
            .Where(m => !settings.ExcludedMembers.Contains(m));
    }
    
    private static object? GetMemberValue(object? obj, MemberInfo member)
    {
        return member switch
        {
            PropertyInfo p => p.GetValue(obj),
            FieldInfo f => f.GetValue(obj),
            _ => throw new ArgumentOutOfRangeException($"Unsupported member type: {member.MemberType}")
        };
    }
    
    private static string Indent(int level) => new('\t', level);
    private static string NewLine => Environment.NewLine;
}