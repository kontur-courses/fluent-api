using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
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
        typeof(Array), typeof(int[])
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
        if (collectionTypes.Contains(type))
            CollectionToString(obj as Array);
        var result = obj + Environment.NewLine;
        if (configuration.TypeConfigs.ContainsKey(type))
            result = configuration.TypeConfigs[type].Aggregate(result, (current, func) => func(current));

        if (finalTypes.Contains(type))
            return result;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            var value = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
            if (configuration.MemberInfoConfigs.ContainsKey(propertyInfo))
                value = configuration.MemberInfoConfigs[propertyInfo]
                    .Aggregate(value, (current, func) => func(current));
                    
            if (configuration.ExcludedTypes.Contains(propertyInfo.PropertyType) || configuration.ExcludedMembers.Contains(propertyInfo))
                continue;
            sb.Append(identation + propertyInfo.Name + " = " + value);
        }

        return sb.ToString();
    }

    private string CollectionToString(Array array)
    {
        var sb = new StringBuilder();
        sb.Append("[");
        foreach (var item in array)
        {
            sb.Append($"{item}, ");
        }

        sb.Append("]");
        return sb.ToString();
    }
}