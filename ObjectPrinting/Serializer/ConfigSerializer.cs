using System;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Serializer.Configs;
using ObjectPrinting.Tools;

namespace ObjectPrinting.Serializer;

public class ConfigSerializer<TOwner>(PrintingConfig<TOwner> config)
{
    public string PrintToString(object? obj, uint nestingLevel)
    {
        if (obj is null)
            return $"null{Environment.NewLine}";
        
        if (TryNonNestingSerialization(obj) is { } serializedObject)
            return serializedObject;
        
        if (nestingLevel + 1 > config.MaxNestingLevel)
            return $"Max nesting level - {config.MaxNestingLevel}" + Environment.NewLine;
        
        var type = obj.GetType();
        var sb = new StringBuilder(type.Name);
        var indentation = new string('\t', (int)(nestingLevel + 1));
        
        foreach (var memberInfo in type.GetMembers().Where(info => info.IsPropertyOrField()))
        {
            if (config.ExcludedTypes.Contains(memberInfo.TryGetType()) ||
                config.ExcludedProperties.Contains(memberInfo.TryGetFullName()))
                continue;
            
            var printingResult = 
                TryCustomSerialization(obj, memberInfo) ?? 
                PrintToString(GetMemberValue(memberInfo, obj), nestingLevel + 1);
            sb.Append(indentation + memberInfo.Name + " = " + TrimIfNeeded(memberInfo, printingResult));
        }
        return sb.ToString();
    }

    private string? TryNonNestingSerialization(object obj)
    {
        if (config.CulturesForTypes.TryGetValue(obj.GetType(), out var culture))
            return ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;
        
        if (obj is string str && config.TrimStringValue is { } trimmed)
            return str[..(int)trimmed] + Environment.NewLine;
        
        return IsFinalType(obj.GetType()) ? obj + Environment.NewLine : null;
    }

    private string? TryCustomSerialization(object obj, MemberInfo memberInfo)
    {
        var value = GetMemberValue(memberInfo, obj);
        if (value is null) return "null" + Environment.NewLine;
        
        if (config.TypeSerializers.TryGetValue(memberInfo.TryGetType(), out var typeSerializer))
            return typeSerializer(value) + Environment.NewLine;
        
        if (config.PropertySerializers.TryGetValue(memberInfo.TryGetFullName(), out var propertySerializer))
            return propertySerializer(value) + Environment.NewLine;
        return null;
    }
    
    private string TrimIfNeeded(MemberInfo memberInfo, string printingResult)
    {
        if (config.TrimmedMembers.TryGetValue(memberInfo.TryGetFullName(), out var length))
            printingResult = printingResult.Length >= length
                ? printingResult[..(int)length] + Environment.NewLine : printingResult;
        return printingResult;
    }
    
    private static bool IsFinalType(Type type)
        => type.IsPrimitive || type == typeof(string) || type == typeof(Guid);
    
    private static object? GetMemberValue(MemberInfo member, object obj)
    {
        if (!member.IsPropertyOrField())
            throw new ArgumentException("Provided member must be Field or Property");

        return member.MemberType == MemberTypes.Field
            ? ((FieldInfo)member).GetValue(obj) : ((PropertyInfo)member).GetValue(obj);
    }
}