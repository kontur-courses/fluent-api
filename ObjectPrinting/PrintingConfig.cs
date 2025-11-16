using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Context;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private static readonly Type[] FinalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    ];

    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<Type, IFormatProvider> numericCultures = new();
    private readonly HashSet<Type> excludedTypes = [];

    private readonly Dictionary<string, Func<object, string>> propertySerializers = new();
    private readonly Dictionary<string, int> stringMaxLengths = new();
    private readonly HashSet<string> excludedProperties = [];

    internal void SetTypeSerializer<T>(Func<T, string> serializer)
    {
        typeSerializers[typeof(T)] = o => serializer((T)o);
    }

    internal void SetNumericCulture<T>(IFormatProvider culture)
    {
        numericCultures[typeof(T)] = culture;
    }

    internal void ExcludeType<T>()
    {
        excludedTypes.Add(typeof(T));
    }

    internal void SetPropertySerializer<T>(string propertyName, Func<T, string> serializer)
    {
        propertySerializers[propertyName] = o => serializer((T)o);
    }

    internal void TrimStringProperty(string propertyName, int maxLength)
    {
        stringMaxLengths[propertyName] = maxLength;
    }

    internal void ExcludeProperty(string propertyName)
    {
        excludedProperties.Add(propertyName);
    }

    public string PrintToString(TOwner obj)
    {
        var visited = new HashSet<object>();
        return PrintToString(obj, 0, visited);
    }

    private string PrintToString(object obj, int nestingLevel, HashSet<object> visited)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (excludedTypes.Contains(type))
            return string.Empty;

        if (!visited.Add(obj))
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return type.Name + " circular dependency" + Environment.NewLine;
            } 
            return obj + Environment.NewLine;
        }
        
        if (TryPrintSimple(type, obj, out var simpleResult))
            return simpleResult;

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return PrintEnumerable((IEnumerable)obj, type, nestingLevel, visited);

        return PrintComplexObject(obj, type, nestingLevel, visited);
    }

    private bool TryPrintSimple(Type type, object obj, out string result)
    {
        if (typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            result = typeSerializer(obj) + Environment.NewLine;
            return true;
        }

        if (FinalTypes.Contains(type))
        {
            if (obj is IFormattable f && numericCultures.TryGetValue(type, out var culture))
                result = f.ToString(null, culture) + Environment.NewLine;
            else
                result = obj + Environment.NewLine;

            return true;
        }

        result = null;
        return false;
    }

    private string PrintComplexObject(object obj, Type type, int nestingLevel, HashSet<object> visited)
    {
        var ident = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        foreach (var propertyInfo in type.GetProperties())
        {
            var propName = propertyInfo.Name;
            var propType = propertyInfo.PropertyType;

            if (excludedProperties.Contains(propName) || excludedTypes.Contains(propType))
                continue;

            var value = propertyInfo.GetValue(obj);

            if (TryPrintPropertyWithCustomOrTrim(sb, ident, propName, propType, value))
                continue;

            sb.Append(ident + propName + " = " +
                      PrintToString(value, nestingLevel + 1, visited));
        }

        return sb.ToString();
    }
    
    private bool TryPrintPropertyWithCustomOrTrim(
        StringBuilder sb,
        string ident,
        string propName,
        Type propType,
        object value)
    {
        if (propertySerializers.TryGetValue(propName, out var propSerializer))
        {
            sb.Append(ident + propName + " = " +
                      propSerializer(value) + Environment.NewLine);
            return true;
        }

        if (propType != typeof(string) || !stringMaxLengths.TryGetValue(propName, out var maxLength)) return false;
        var s = (string)value;
        if (s != null && s.Length > maxLength)
            s = s[..maxLength];

        sb.Append(ident + propName + " = " + s + Environment.NewLine);
        return true;

    }

    private string PrintEnumerable(IEnumerable enumerable, Type type, int nestingLevel, HashSet<object> visited)
    {
        var sb = new StringBuilder();
        var ident = new string('\t', nestingLevel + 1);

        sb.AppendLine(type.Name);

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                sb.Append(ident + "null" + Environment.NewLine);
                continue;
            }

            var itemType = item.GetType();

            if (excludedTypes.Contains(itemType))
                continue;

            var printed = PrintToString(item, nestingLevel + 1, visited);
            sb.Append(ident + printed);
        }

        return sb.ToString();
    }
    
    public IPrintingConfigContext<TOwner, T> For<T>()
    {
        return new BaseContext<TOwner, T>(this);
    }

    public IPropertyPrinterConfigContext<TOwner, T> For<T>(Expression<Func<TOwner, T>> propertySelector)
    {
        var member = (MemberExpression)propertySelector.Body;
        var propName = member.Member.Name;
        return new PropertyContext<TOwner, T>(this, propName);
    }

    public IStringPropertyPrinterConfigContext<TOwner> For(Expression<Func<TOwner, string>> propertySelector)
    {
        var member = (MemberExpression)propertySelector.Body;
        var propName = member.Member.Name;
        return new StringPropertyContext<TOwner>(this, propName);
    }
}
