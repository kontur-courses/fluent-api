using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class SerializeConfig<TOwner>
{
    public HashSet<Type> ExcludedTypes { get; private set; } = new();
    public HashSet<string> ExcludedProperties { get; private set; } = new();
    public Dictionary<Type, Delegate> TypeSerializers { get; private set; } = new();
    public Dictionary<string, Delegate> PropertySerializers { get; private set; } = new();
    public Dictionary<Type, CultureInfo> TypeCultures { get; private set; } = new();
    public Dictionary<string, CultureInfo>  PropertyCultures { get; private set; } = new();
    public Dictionary<string, int> StringPropertiesMaxLen { get; private set; } = new();
    public int? StringMaxLen {  get; private set; } = null;


    public void SetStringMaxLen(int maxLen)
    {
        if (!IsMaxLenCorrect(maxLen))
            throw new ArgumentException($"{nameof(maxLen)} must be greater or equal to zero");
        StringMaxLen = maxLen;
    }

    public void SetMaxLenForPropertyFromExpression<TPropType>(
        Expression<Func<TOwner, TPropType>> propSelector,
        int maxLen)
    {
        if (!IsMaxLenCorrect(maxLen))
            throw new ArgumentException($"{nameof(maxLen)} must be greater or equal to zero");

        var name = GetNameOfProperty(propSelector);
        StringPropertiesMaxLen[name] = maxLen;
    }

    public void AddExcludedPropertyFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        var name = GetNameOfProperty(propSelector);
        ExcludedProperties.Add(name);
    }

    public void SetSerializerForPropertyFromExpression<TPropType>(
        Expression<Func<TOwner, TPropType>> propSelector,
        Delegate serializer)
    {
        var name = GetNameOfProperty(propSelector);
        PropertySerializers[name] = serializer;
    }

    public void SetCultureForPropertyFromExpression<TPropType>(
        Expression<Func<TOwner, TPropType>> propSelector,
        CultureInfo culture)
    {
        var name = GetNameOfProperty(propSelector);
        PropertyCultures[name] = culture;
    }

    private string GetNameOfProperty<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        if (propSelector.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (propSelector.Body is UnaryExpression unary && unary.Operand is MemberExpression operand)
            return operand.Member.Name;

        throw new ArgumentException("Invalid expression for property selector");
    }

    private bool IsMaxLenCorrect(int maxLen) => maxLen >= 0;
}
