using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public class PrintingConfig<TOwner> : ISerializerSetter
{
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers = new();
    
    private readonly HashSet<PropertyInfo> excludedProps = new();
    private readonly HashSet<Type> excludedTypes = new();
        
    public PrintingConfig<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression)
    {
        excludedProps.Add(GetPropertyInfoFromExpression(expression));
        return this;
    }
        
    public IPropertySpecifier<T, TOwner> Serialize<T>()
    {
        return new PropertySpecifier<T, TOwner>(this);
    }

    public IPropertySpecifier<T, TOwner> Serialize<T>(Expression<Func<TOwner, T>> expression)
    {
        return new PropertySpecifier<T, TOwner>(this, GetPropertyInfoFromExpression(expression));
    }
    
    public PrintingConfig<TOwner> SetCulture<T>(CultureInfo cultureInfo)
        where T: IFormattable
    {
        return Serialize<T>()
            .With(obj => string.Format(cultureInfo, "{0}", obj));
    }
    
    public PrintingConfig<TOwner> SetCulture<T>(Expression<Func<TOwner, T>> expression, CultureInfo cultureInfo)
        where T: IFormattable
    {
        return Serialize(expression)
            .With(obj => string.Format(cultureInfo, "{0}", obj));
    }
    
    public PrintingConfig<TOwner> SliceStrings(int maxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentException("Parameter maxLength must be positive");
        
        return Serialize<string>()
            .With(s => maxLength >= s.Length ? s : s[..maxLength]);
    }
        
    void ISerializerSetter.SetSerializer<T>(Func<T, string> serializer, PropertyInfo propertyInfo)
    {
        if (propertyInfo is not null)
            propertySerializers[propertyInfo] = obj => serializer((T) obj);
        else
            typeSerializers[typeof(T)] = obj => serializer((T) obj);
    }

    internal bool TrySerializeValueType(Type type, object obj, out string serialized)
    {
        serialized = "";
        
        if (obj is not string && !type.IsValueType)
            return false;
        
        serialized = typeSerializers.TryGetValue(type, out var serializer) 
            ? serializer(obj) 
            : obj.ToString();

        return true;
    }

    internal bool IsExcluded(PropertyInfo propertyInfo)
    {
        return excludedProps.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);
    }

    internal bool TrySerializeProperty(object obj, PropertyInfo propertyInfo, out string serialized)
    {
        serialized = "";

        if (!propertySerializers.TryGetValue(propertyInfo, out var serializer))
            return false;

        serialized = serializer(propertyInfo.GetValue(obj));
        return true;
    }

    private static PropertyInfo GetPropertyInfoFromExpression<T>(Expression<Func<TOwner, T>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"Expression {expression} is not MemberExpression");
            
        return memberExpression.Member as PropertyInfo;
    }
}