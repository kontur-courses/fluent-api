using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

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
        
    public ISerializer<T, TOwner> Select<T>()
    {
        return new Serializer<T, TOwner>(this);
    }

    public ISerializer<T, TOwner> Select<T>(Expression<Func<TOwner, T>> expression)
    {
        return new Serializer<T, TOwner>(this, GetPropertyInfoFromExpression(expression));
    }
    
    public PrintingConfig<TOwner> SetCulture<T>(CultureInfo cultureInfo)
        where T: IFormattable
    {
        return Select<T>()
            .Serialize(obj => string.Format(cultureInfo, "{0}", obj));
    }
    
    public PrintingConfig<TOwner> SetCulture<T>(Expression<Func<TOwner, T>> expression, CultureInfo cultureInfo)
        where T: IFormattable
    {
        return Select(expression)
            .Serialize(obj => string.Format(cultureInfo, "{0}", obj));
    }
    
    public PrintingConfig<TOwner> SliceStrings(int maxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentException("Parameter maxLength must be positive");
        
        return Select<string>()
            .Serialize(s => maxLength >= s.Length ? s : s[..maxLength]);
    }
        
    void ISerializerSetter.SetSerializer<T>(Func<T, string> serializer, PropertyInfo propertyInfo)
    {
        if (propertyInfo is not null)
            propertySerializers[propertyInfo] = obj => serializer((T) obj);
        else
            typeSerializers[typeof(T)] = obj => serializer((T) obj);
    }
        
    public string PrintToString(TOwner obj)
    {
        return PrintToString1(obj, new List<object>().ToImmutableList());
    }
    
    public string PrintToString1(object obj, ImmutableList<object> previous)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();

        if (TrySerializeValueType(type, obj, out var serializedValue))
            return serializedValue;

        if (IsCyclic(obj, previous))
            return "cyclic link";

        if (obj is IDictionary dictionary)
            return dictionary.ObjectPrintDictionary(this, previous);

        if (obj is IEnumerable enumerable)
            return enumerable.Cast<object>().ObjectPrintEnumerable(this, previous);

        var identation = new string('\t', previous.Count + 1);
        previous = previous.Add(obj);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name + " (");

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (IsExcluded(property))
                continue;
            
            sb.Append($"{identation}{property.Name}: ");

            if (TrySerializeProperty(obj, property, out var serialized)) 
                sb.Append(serialized);
            else
                sb.Append(PrintToString1(property.GetValue(obj), previous));
            
            sb.AppendLine(";");
        }
        
        sb.Append(new string('\t', previous.Count - 1) + ")");

        return sb.ToString();
    }

    private bool TrySerializeValueType(Type type, object obj, out string serialized)
    {
        serialized = "";
        
        if (obj is not string && !type.IsValueType)
            return false;
        
        serialized = typeSerializers.TryGetValue(type, out var serializer) 
            ? serializer(obj) 
            : obj.ToString();

        return true;
    }

    private bool IsCyclic(object current, IEnumerable<object> previous)
    {
        return previous.Any(e => ReferenceEquals(e, current));
    }
    
    private bool IsExcluded(PropertyInfo propertyInfo)
    {
        return excludedProps.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);
    }

    private bool TrySerializeProperty(object obj, PropertyInfo propertyInfo, out string serialized)
    {
        serialized = "";

        if (!propertySerializers.TryGetValue(propertyInfo, out var serializer))
            return false;

        serialized = serializer(propertyInfo.GetValue(obj));
        return true;
    }

    private PropertyInfo GetPropertyInfoFromExpression<T>(Expression<Func<TOwner, T>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"Expression {expression} is not MemberExpression");
            
        return memberExpression.Member as PropertyInfo;
    }
}