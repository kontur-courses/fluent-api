using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Actions;

namespace ObjectPrinting;

public class PrintingConfigurator<TOwner>
{
    private readonly PrintingConfig config = new();

    private PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TOwner, TProperty>> expression)
    {
        var property = expression.Body as MemberExpression;
        var propertyInfo = property?.Member as PropertyInfo;
        return propertyInfo ?? throw new ArgumentException("Invalid expression");
    }

    public PrintingConfigurator<TOwner> Exclude<T>()
    {
        var action = new ExcludeAction(typeof(T));
        config.AddAction(action);
        return this;
    }

    public PrintingConfigurator<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> expression)
    {
        var propertyInfo = GetPropertyInfo(expression);
        var action = new ExcludeAction(propertyInfo);
        config.AddAction(action);
        return this;
    }

    public PrintingConfigurator<TOwner> SpecifyCulture<T>(CultureInfo newCulture) where T : IFormattable
    {
        var action = new SpecifyCultureAction(typeof(T), newCulture);
        config.AddAction(action);
        return this;
    }

    public PrintingConfigurator<TOwner> SpecifyCulture<TProperty>(Expression<Func<TOwner, TProperty>> expression,
        CultureInfo cultureInfo)
    {
        var propertyInfo = GetPropertyInfo(expression);
        var action = new SpecifyCultureAction(propertyInfo, cultureInfo);
        config.AddAction(action);
        return this;
    }

    public PrintingConfigurator<TOwner> AddSerializer<T>(Func<object, string> serializer)
    {
        var action = new SerializeAction(typeof(T), serializer);
        config.AddAction(action);
        return this;
    }

    public PrintingConfigurator<TOwner> AddSerializer<TProperty>(Expression<Func<TOwner, TProperty>> expression,
        Func<object, string> serializer)
    {
        var propertyInfo = GetPropertyInfo(expression);
        var action = new SerializeAction(propertyInfo, serializer);
        config.AddAction(action);
        return this;
    }

    public PrintingConfigurator<TOwner> Trim(Expression<Func<TOwner, string>> expression, int maxLength)
    {
        var property = expression.Body as MemberExpression;
        var propertyInfo = property?.Member as PropertyInfo;
        var action = new TrimAction(propertyInfo, maxLength);
        config.AddAction(action);
        return this;
    }

    public ObjectPrinter Build() => new ObjectPrinter(config);
}