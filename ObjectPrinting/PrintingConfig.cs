﻿using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly List<PropertyInfo> excludedProperties = new();
    private readonly List<Type> excludedTypes = new();

    private readonly Type[] finalTypes =
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var body = memberSelector.Body;
        if (!IsPropertyCall(body))
            throw new ArgumentException($"Member selector ({memberSelector}) isn't property call");

        if ((body as MemberExpression)!.Member is PropertyInfo property)
            excludedProperties.Add(property);

        return this;
    }

    private static bool IsPropertyCall(Expression expression)
    {
        var isMemberCall = expression.NodeType == ExpressionType.MemberAccess;
        if (!isMemberCall) return false;

        var isPropertyCall = (expression as MemberExpression)!.Member.MemberType == MemberTypes.Property;
        return isPropertyCall;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj is null)
            return "null";

        if (finalTypes.Contains(obj.GetType()))
            return $"{obj}";

        var sb = new StringBuilder();

        var type = obj.GetType();
        sb.Append(type.Name);

        var identation = new string('\t', nestingLevel + 1);
        foreach (var property in type.GetProperties().Where(NotExcluded))
        {
            var print = PrintToString(property.GetValue(obj), nestingLevel + 1);
            sb.Append($"{Environment.NewLine}{identation}{property.Name} = {print}");
        }

        return sb.ToString();
    }

    private bool NotExcluded(PropertyInfo property)
    {
        return !excludedProperties.Contains(property) && !excludedTypes.Contains(property.PropertyType);
    }
}