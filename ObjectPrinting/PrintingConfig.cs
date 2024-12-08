using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private HashSet<object?> printedObjects;
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<PropertyInfo?> excludedProperties = [];

    internal Dictionary<Type, CultureInfo> CulturesForTypes { get; } = new();
    internal Dictionary<Type, Delegate> CustomTypeSerializers { get; } = new();
    internal Dictionary<PropertyInfo, Delegate> CustomPropertySerializers { get; } = new();
    internal Dictionary<PropertyInfo, int> TrimmedProperties { get; } = new();
    
    public PropertyPrintingConfig<TOwner, TPropType> SetPrintingFor<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> SetPrintingFor<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)expression.Member);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        excludedProperties.Add((PropertyInfo)expression.Member);
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        return this;
    }

    public string PrintToString(TOwner? obj)
    {
        printedObjects = [];
        return PrintToString(obj, 0);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (printedObjects.Contains(obj))
        {
            return "Cycle reference detected";
        }
        printedObjects.Add(obj);
        
        if (obj == null)
            return "null" + Environment.NewLine;

        var indentation = new string('\t', nestingLevel + 1);
        
        if (CulturesForTypes.TryGetValue(obj.GetType(), out var culture))
            return indentation + ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;

        if (IsFinal(obj.GetType()))
        {
            return obj + Environment.NewLine;
        }

        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        
        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                continue;

            string printingResult;

            if (CustomTypeSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
            {
                printingResult = (string)typeSerializer.DynamicInvoke(propertyInfo.GetValue(obj));
            }
            else if (CustomPropertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
            {
                printingResult = (string)propertySerializer.DynamicInvoke(propertyInfo.GetValue(obj));
            }
            else
                printingResult = PrintToString(propertyInfo.GetValue(obj),
                    nestingLevel + 1);

            if (TrimmedProperties.TryGetValue(propertyInfo, out var length))
                printingResult = printingResult.Length > length
                    ? printingResult[..length] + Environment.NewLine
                    : printingResult;

            sb.Append(indentation + propertyInfo.Name + " = " + printingResult);
        }
        return sb.ToString();
    }
    
    private static bool IsFinal(Type type) 
        => type.IsValueType || type == typeof(string);
}
