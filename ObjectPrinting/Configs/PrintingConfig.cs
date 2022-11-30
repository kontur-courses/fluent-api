using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly ObjectConfiguration<TOwner> configuration;
    private readonly Type[] finalTypes = {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };
        
    public PrintingConfig(ObjectConfiguration<TOwner> configuration)
    {
        this.configuration = configuration;
    }
        
    public string PrintToString(TOwner obj) =>
        PrintToString(obj, 0);

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj is null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();
        if (obj is IEnumerable enumerable and not string)
            return ParseCollection(enumerable, nestingLevel);
        if (configuration.TypeConfigs.ContainsKey(type))
            return configuration.TypeConfigs[type](obj);

        if (finalTypes.Contains(type))
            return obj.ToString();

        var sb = new StringBuilder();

        if (!type.IsGenericType && type != typeof(Guid))
            sb.AppendLine(type.Name);
        else if (type == typeof(Guid))
            sb.Append(type.Name);
        if (type != typeof(Guid))
            sb.Append(GetElements(type.GetFields(), nestingLevel, obj));
        sb.Append(GetElements(type.GetProperties(), nestingLevel, obj));

        return sb.ToString();
    }

    private string GetElements(IEnumerable<MemberInfo> memberInfos, int nestingLevel, object obj)
    {
        var builder = new StringBuilder();
        foreach (var memberInfo in memberInfos)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            var fieldInfo = memberInfo as FieldInfo;
            var identation = new string('\t', nestingLevel + 1);

            var value = PrintToString(propertyInfo == null ? fieldInfo!.GetValue(obj) : propertyInfo.GetValue(obj),
                nestingLevel + 1);
            if (configuration.MemberInfoConfigs.ContainsKey(memberInfo))
                value = configuration.MemberInfoConfigs[memberInfo](value);

            if (propertyInfo is not null && (configuration.ExcludedTypes.Contains(propertyInfo.PropertyType) ||
                                         configuration.ExcludedMembers.Contains(propertyInfo))
                || fieldInfo is not null && configuration.ExcludedTypes.Contains(fieldInfo.FieldType) ||
                configuration.ExcludedMembers.Contains(fieldInfo)) 
                continue;
            builder.Append(identation + memberInfo.Name + " = " + value + "\n");           
        }

        return builder.ToString();
    }

    private string ParseCollection(IEnumerable enumerable, int nestingLevel)
    {
        var result = new StringBuilder();
        result.Append("[");
        foreach (var item in enumerable)
        {
            var stringToPrint = PrintToString(item, nestingLevel) + (finalTypes.Contains(item.GetType()) ? ", " : "");
            if (!finalTypes.Contains(item.GetType()))
                result.Append("\n");
            result.Append(stringToPrint);
        }
        
        if (result[^2] == ',')
            result.Remove(result.Length - 2, 2);
        result.Append("]");
        return result.ToString();
    }
}