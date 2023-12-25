using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
{
    private readonly Dictionary<Type, CultureInfo> cultures = new();
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers = new();
    private readonly Dictionary<PropertyInfo, int> proprtiesToTrim = new();
    private readonly HashSet<PropertyInfo> excludedProperties = new();
    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<Type> finalTypes = new()
    {
        typeof(int),
        typeof(double),
        typeof(float),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan)
    };

    Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypeSerializers => typeSerializers;
    Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.PropertiesToTrim => proprtiesToTrim;
    Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.PropertySerializers => propertySerializers;
    Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.Cultures => cultures;

    public TypePrintingConfig<TOwner, TType> For<TType>()
    {
        return new TypePrintingConfig<TOwner, TType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> For<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyInfo(memberSelector));
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var property = GetPropertyInfo(memberSelector);
        excludedProperties.Add(property);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    private PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            return (PropertyInfo)member.Member;
        }
        throw new ArgumentException("please, choose property");
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0, new HashSet<object>());
    }

    private string PrintToString(object obj, int nestingLevel, HashSet<object> printed)
    {
        if (obj == null)
            return "null" + Environment.NewLine;
        var type = obj.GetType();

        if (finalTypes.Contains(type))
            return obj + Environment.NewLine;

        if (printed.Contains(obj))
            return "this " + type.Name + Environment.NewLine;
        printed.Add(obj);

        if (obj is IEnumerable<object> collection)
            return PrintCollection(collection, nestingLevel, printed);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return PrintDictionary(obj, nestingLevel, printed);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        sb = PrintNestingElements(sb, obj, nestingLevel, printed);
        return sb.ToString();
    }

    private string PrintCollection(IEnumerable<object> collection, int nestingLevel, HashSet<object> printed)
    {
        var sb = new StringBuilder();
        var identation = new string('\t', nestingLevel + 1);
        if (!collection.GetEnumerator().MoveNext())
            return ("Empty" + Environment.NewLine);
        foreach (var item in collection)
        {
            sb.Append(identation + PrintToString(item, nestingLevel + 1, printed));
        }
        return sb.ToString();
    }

    private string PrintDictionary(object obj, int nestingLevel, HashSet<object> printed)
    {
        var dict = (IDictionary)obj;
        if (!dict.GetEnumerator().MoveNext())
            return ("Empty" + Environment.NewLine);
        var sb = new StringBuilder();
        var identation = new string('\t', nestingLevel + 1);
        foreach (var pair in dict)
        {
            var key = ((DictionaryEntry)pair).Key;
            var value = ((DictionaryEntry)pair).Value;
            sb.Append(identation + PrintToString(key, nestingLevel, printed).Trim() 
                + " : " 
                + PrintToString(value, nestingLevel, printed).Trim() 
                + Environment.NewLine);
        }
        return sb.ToString();
    }

    private StringBuilder PrintNestingElements(StringBuilder sb, object obj, int nestingLevel, HashSet<object> printed)
    {
        var type = obj.GetType();
        var identation = new string('\t', nestingLevel + 1);
        foreach (var property in type.GetProperties())
        {
            if (!excludedTypes.Contains(property.PropertyType) && !excludedProperties.Contains(property))
                sb.Append(identation + property.Name 
                    + " = " 
                    + PrintProperty(property, property.GetValue(obj), nestingLevel + 1, printed));
        }
        return sb;
    }

    private string PrintProperty(PropertyInfo property, object obj, int nestingLevel, HashSet<object> printed)
    {
        string result = null;
        if (cultures.TryGetValue(property.PropertyType, out var culture))
            result = string.Format(culture, "{0}", obj);
        if (typeSerializers.TryGetValue(property.PropertyType, out var typeSerialize))
            result = typeSerialize.Invoke(obj);
        if (propertySerializers.TryGetValue(property, out var propertySerialize))
            result = propertySerialize.Invoke(obj);
        if (proprtiesToTrim.TryGetValue(property, out var trim))
        {
            result ??= obj as string;
            result = result[..Math.Min(result.Length, trim)];
        }

        return result is null ? PrintToString(obj, nestingLevel, printed) : result + Environment.NewLine;
    }
}