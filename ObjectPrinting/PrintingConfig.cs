using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return PrintToString(obj, 0, visited, "");
    }

    private string PrintToString(object obj, int nestingLevel, HashSet<object> visited, string currentPath)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (excludedTypes.Contains(type))
            return string.Empty;
        
        if (TryPrintSimple(type, obj, out var simple))
            return simple;

        if (!visited.Add(obj))
            return "circular dependency" + Environment.NewLine;
        
        if (obj is IDictionary dict)
            return PrintDictionary(dict, type, nestingLevel, visited, currentPath);
        
        if (typeof(IEnumerable).IsAssignableFrom(type))
            return PrintEnumerable((IEnumerable)obj, type, nestingLevel, visited, currentPath);

        return PrintComplexObject(obj, type, nestingLevel, visited, currentPath);
    }
    
    private string PrintDictionary(IDictionary dict, Type type, int nestingLevel, HashSet<object> visited, string currentPath)
    {
        var sb = new StringBuilder();
        var ident = new string('\t', nestingLevel + 1);
        sb.AppendLine(type.Name);

        foreach (DictionaryEntry entry in dict)
        {
            var keyPath = currentPath + "[key]";
            var valuePath = currentPath + "[value]";

            var keyPrinted = PrintToString(entry.Key, nestingLevel + 1, visited, keyPath).TrimEnd('\r', '\n');
            var valuePrinted = PrintToString(entry.Value, nestingLevel + 1, visited, valuePath).TrimEnd('\r', '\n');

            sb.AppendLine($"{ident}Key = {keyPrinted}, Value = {valuePrinted}");
        }

        return sb.ToString();
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

    private string PrintComplexObject(object obj, Type type, int nestingLevel, HashSet<object> visited, string currentPath)
    {
        var ident = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        var flags = BindingFlags.Public | BindingFlags.Instance;
        
        foreach (var propertyInfo in type.GetProperties(flags))
        {
            var propName = propertyInfo.Name;
            var propType = propertyInfo.PropertyType;

            var fullPropPath = string.IsNullOrEmpty(currentPath)
                ? propName
                : currentPath + "." + propName;

            if (excludedProperties.Contains(fullPropPath) || excludedTypes.Contains(propType))
                continue;

            var value = propertyInfo.GetValue(obj);

            if (TryPrintPropertyWithCustomOrTrim(sb, ident, fullPropPath, propType, value))
                continue;

            sb.Append(ident + propName + " = " +
                      PrintToString(value, nestingLevel + 1, visited, fullPropPath));
        }
        
        foreach (var fieldInfo in type.GetFields(flags))
        {
            var fieldName = fieldInfo.Name;
            var fieldType = fieldInfo.FieldType;

            var fullFieldPath = string.IsNullOrEmpty(currentPath)
                ? fieldName
                : currentPath + "." + fieldName;

            if (excludedProperties.Contains(fullFieldPath) || excludedTypes.Contains(fieldType))
                continue;

            var value = fieldInfo.GetValue(obj);
            
            if (TryPrintPropertyWithCustomOrTrim(sb, ident, fullFieldPath, fieldType, value))
                continue;

            sb.Append(ident + fieldName + " = " +
                      PrintToString(value, nestingLevel + 1, visited, fullFieldPath));
        }

        return sb.ToString();
    }
    
    private bool TryPrintPropertyWithCustomOrTrim(StringBuilder sb, string ident, string fullPropPath, Type propType, object value)
    {
        if (propertySerializers.TryGetValue(fullPropPath, out var propSerializer))
        {
            sb.Append(ident + fullPropPath + " = " +
                      propSerializer(value) + Environment.NewLine);
            return true;
        }

        if (propType != typeof(string) || !stringMaxLengths.TryGetValue(fullPropPath, out var maxLength))
            return false;

        var s = (string)value;
        if (s != null && s.Length > maxLength)
            s = s[..maxLength];

        sb.Append(ident + fullPropPath + " = " + s + Environment.NewLine);
        return true;
    }
    
    private string PrintEnumerable(IEnumerable enumerable, Type type, int nestingLevel, HashSet<object> visited, string currentPath)
    {
        var sb = new StringBuilder();
        var ident = new string('\t', nestingLevel + 1);
        sb.AppendLine(type.Name);
        var index = 0;

        foreach (var item in enumerable)
        {
            var itemPath = currentPath + "[" + index + "]";
            index++;

            if (item == null)
            {
                sb.Append(ident + "null" + Environment.NewLine);
                continue;
            }
            var itemType = item.GetType();

            if (excludedTypes.Contains(itemType))
                continue;

            var printed = PrintToString(item, nestingLevel + 1, visited, itemPath);
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
        var propPath = GetPropertyPath(propertySelector);
        return new PropertyContext<TOwner, T>(this, propPath);
    }

    public IStringPropertyPrinterConfigContext<TOwner> For(Expression<Func<TOwner, string>> propertySelector)
    {
        var propPath = GetPropertyPath(propertySelector);
        return new StringPropertyContext<TOwner>(this, propPath);
    }
    
    private static string GetPropertyPath(LambdaExpression expression)
    {
        var parts = new List<string>();
        var current = expression.Body;

        while (current is MemberExpression m)
        {
            parts.Add(m.Member.Name);
            current = m.Expression;
        }

        parts.Reverse();
        return string.Join(".", parts);
    }
}
