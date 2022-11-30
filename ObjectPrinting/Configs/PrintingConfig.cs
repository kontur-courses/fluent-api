using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private readonly Type[] collectionTypes =
    {
        typeof(IEnumerable)
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
        var result = obj + Environment.NewLine;
        if (obj is IEnumerable enumerable and not string)
            return CollectionToString(enumerable);
        if (configuration.TypeConfigs.ContainsKey(type))
            result = ApplyFunc(configuration.TypeConfigs[type], obj);

        if (finalTypes.Contains(type))
            return result;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            var value = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
            if (configuration.MemberInfoConfigs.ContainsKey(propertyInfo))
                value = ApplyFunc(configuration.MemberInfoConfigs[propertyInfo], value);
                    
            if (configuration.ExcludedTypes.Contains(propertyInfo.PropertyType) || configuration.ExcludedMembers.Contains(propertyInfo))
                continue;
            sb.Append(identation + propertyInfo.Name + " = " + value);
        }

        return sb.ToString();
    }

    private static string ApplyFunc(List<Func<object, string>> configurationMemberInfoConfig, object value)
    {
        var result = new StringBuilder();
        foreach (var func in configurationMemberInfoConfig)
            result.Append(func(value));
        
        return result.ToString();
    }

    private string CollectionToString(IEnumerable enumerable)
    {
        var sb = new StringBuilder();
        sb.Append("[");
        foreach (var item in enumerable)
        {
            if (item is IEnumerable enumerate and not string)
                sb.Append(CollectionToString(enumerate));
            else
            {
                // TODO: Change Binary
                var type = item.GetType();
                if (configuration.TypeConfigs.ContainsKey(type))
                    sb.Append(ApplyFunc(configuration.TypeConfigs[type], item));
                else
                    sb.Append(item);
                sb.Append(", ");
            }
        }

        sb.Remove(sb.Length - 2, 2);
        sb.AppendLine("]");
        return sb.ToString();
    }
}