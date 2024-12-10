using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;

namespace ObjectPrinting;

public class SerializeConfig<TOwner>
{
    public readonly HashSet<Type> ExcludedTypes = new();
    public readonly HashSet<string> ExcludedProperties = new();
    public readonly Dictionary<Type, Func<object, string>> TypeSerializers = new();
    public readonly Dictionary<string, Func<object, string>> PropertySerializers = new();
    public readonly Dictionary<Type, CultureInfo> TypeCultures = new();
    public readonly Dictionary<string, CultureInfo> PropertyCultures = new();
    public readonly Dictionary<string, int> StringPropertiesMaxLen = new();
    public int? StringMaxLen {  get; private set; } = null;

    internal void SetStringMaxLen(int maxLen)
    {
        EnsureMaxLenCorrect(maxLen);
        StringMaxLen = maxLen;
    }

    internal void SetMaxLenForPropertyFromExpression<TPropType>(
        Expression<Func<TOwner, TPropType>> propSelector,
        int maxLen)
    {
        EnsureMaxLenCorrect(maxLen);

        var name = GetNameOfProperty(propSelector);
        StringPropertiesMaxLen[name] = maxLen;
    }

    internal void AddExcludedPropertyFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        var name = GetNameOfProperty(propSelector);
        ExcludedProperties.Add(name);
    }

    internal void SetSerializerForPropertyFromExpression<TPropType>(
        Expression<Func<TOwner, TPropType>> propSelector,
        Func<object, string> serializer)
    {
        var name = GetNameOfProperty(propSelector);
        PropertySerializers[name] = serializer;
    }

    internal void SetCultureForPropertyFromExpression<TPropType>(
        Expression<Func<TOwner, TPropType>> propSelector,
        CultureInfo culture)
        where TPropType : INumber<TPropType>
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

    private void EnsureMaxLenCorrect(int maxLen)
    {
        if (maxLen < 0)
            throw new ArgumentException($"{nameof(maxLen)} must be greater or equal to zero");
    }
}
