using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> finalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    ];
    private readonly HashSet<Type> excludedTypes = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializationMethods = [];
    private readonly Dictionary<Type, IFormatProvider> typeCultures = [];
    private readonly Dictionary<string, Func<object, string>> propertySerializationMethods = [];
    
    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
        {
            return "null" + Environment.NewLine;
        }

        if (finalTypes.Contains(obj.GetType()))
            return obj + Environment.NewLine;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            sb.Append(identation + propertyInfo.Name + " = " +
                      PrintToString(propertyInfo.GetValue(obj),
                          nestingLevel + 1));
        }

        return sb.ToString();
    }

    public PrintingConfig<TOwner> ExcludeType<TType>()
    {
        excludedTypes.Add(typeof(TType));
        return this;
    }

    public PrintingConfig<TOwner> AddSerializationMethod<TType>(Func<TType, string> serializationMethod)
    {
        typeSerializationMethods[typeof(TType)] = obj => serializationMethod((TType)obj);
        return this;
    }

    public PrintingConfig<TOwner> SpecifyTheCulture<TType>(IFormatProvider culture)
    {
        typeCultures[typeof(TType)] = culture;
        return this;
    }

    public PrintingConfig<TOwner> AddSerializationMethod<TType>(
        Func<TType, string> serializationMethod,
        Expression<Func<TOwner, TType>> property)
    {
        var propertyName = GetPropertyName(property);
        propertySerializationMethods[propertyName] = obj => serializationMethod((TType)obj);
        return this;
    }
    
    private static string GetPropertyName<T>(Expression<Func<TOwner, T>> property)
    {
        if (property.Body is MemberExpression member)
        {
            return member.Member.Name;
        }
        
        throw new ArgumentException();
    }
}