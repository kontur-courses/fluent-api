using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<PropertyInfo> excludedProperties = new();

    private HashSet<object> printedObjects;

    internal Dictionary<Type, Delegate> CustomTypeSerializers { get; } = new();

    internal Dictionary<PropertyInfo, Delegate> CustomPropertySerializers { get; } = new();

    internal Dictionary<Type, CultureInfo> CulturesForTypes { get; } = new();

    internal Dictionary<PropertyInfo, int> TrimmedProperties { get; } = new();

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>()
    {
        return new MemberPrintingConfig<TOwner, TMemberType>(this);
    }

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        return new MemberPrintingConfig<TOwner, TMemberType>(this, (PropertyInfo)expression.Member);
    }

    public PrintingConfig<TOwner> ExcludeProperty<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        excludedProperties.Add((PropertyInfo)expression.Member);
        return this;
    }

    public PrintingConfig<TOwner> ExcludePropertyType<TMemberType>()
    {
        excludedTypes.Add(typeof(TMemberType));
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        printedObjects = new HashSet<object>();
        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (printedObjects.Contains(obj) && !obj.GetType().IsValueType)
            return "Cycle reference detected" + Environment.NewLine;
        printedObjects.Add(obj);

        if (obj == null)
            return "null" + Environment.NewLine;

        var indentation = new string('\t', nestingLevel + 1);

        if (CulturesForTypes.TryGetValue(obj.GetType(), out var culture))
            return indentation + ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;

        if (IsTypeFinal(obj.GetType()))
            return obj + Environment.NewLine;

        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);

        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                continue;

            var printingResult = GetPrintingResult(obj, propertyInfo, nestingLevel);

            sb.Append(indentation + propertyInfo.Name + " = " + printingResult);
        }

        return sb.ToString();
    }

    private string GetPrintingResult(Object obj, PropertyInfo propertyInfo, int nestingLevel)
    {
        string printingResult;

        if (CustomTypeSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
        {
            printingResult = typeSerializer.DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
        }
        else if (CustomPropertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
        {
            printingResult = propertySerializer.DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine;
        }
        else
            printingResult = PrintToString(propertyInfo.GetValue(obj),
                nestingLevel + 1);

        return TrimResultIfNeeded(propertyInfo, printingResult);
    }

    private string TrimResultIfNeeded(PropertyInfo propertyInfo, string printingResult)
    {
        if (TrimmedProperties.TryGetValue(propertyInfo, out var length))
            printingResult = printingResult.Substring(0, length) + Environment.NewLine;

        return printingResult;
    }

    private static bool IsTypeFinal(Type type)
    {
        return type.IsValueType || type == typeof(string);
    }
}