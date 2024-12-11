using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly List<Type> excludingTypes = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<Type, CultureInfo> typeCultures = new();
    private readonly Dictionary<string, Func<object, string>> propertySerializers = new();
    private readonly HashSet<string> excludingProperties = [];
        
    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludingTypes.Add(typeof(TPropType));
        return this;
    }
    
    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is MemberExpression memberExpression)
        {
            excludingProperties.Add(memberExpression.Member.Name);
        }
        else
        {
            throw new ArgumentException("Expression must be a member access expression", nameof(memberSelector));
        }

        return this;
    }
        
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }
    
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
    }
        
    public void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer)
    {
        typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);
    }

    public void AddPropertySerializer(string propertyName, Func<object, string> serializer)
    {
        propertySerializers[propertyName] = serializer;
    }

    public void AddTypeCulture<TPropType>(CultureInfo culture)
    {
        typeCultures[typeof(TPropType)] = culture;
    }
        
    public string PrintToString(TOwner obj)
    {
        return PrintToStringInternal(obj, 0);
    }

    private string PrintToStringInternal(object? obj, int nestingLevel, HashSet<object>? visitedObjects = null)
    {
        if (obj is null)
            return HandleNull();

        visitedObjects ??= [];

        if (!visitedObjects.Add(obj))
            return HandleCyclicReference();

        var type = obj.GetType();

        if (typeSerializers.TryGetValue(type, out var serializer))
            return HandleCustomSerialization(obj, serializer);

        if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
            return HandleFormattable(obj, culture);

        if (IsFinalType(type))
            return obj + Environment.NewLine;

        if (obj is IDictionary dictionary)
            return HandleDictionary(dictionary, nestingLevel, visitedObjects);

        if (obj is IEnumerable enumerable)
            return HandleEnumerable(enumerable, nestingLevel, visitedObjects);

        return HandleComplexObject(obj, type, nestingLevel, visitedObjects);
    }

    private static string HandleNull()
    {
        return "null" + Environment.NewLine;
    }

    private static string HandleCyclicReference()
    {
        return "(cyclic reference detected)" + Environment.NewLine;
    }

    private static string HandleCustomSerialization(object obj, Func<object, string> serializer)
    {
        return serializer(obj) + Environment.NewLine;
    }

    private static string HandleFormattable(object obj, IFormatProvider culture)
    {
        return ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;
    }

    private static bool IsFinalType(Type type)
    {
        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        return finalTypes.Contains(type) || type.IsEnum;
    }

    private string HandleDictionary(IDictionary dictionary, int nestingLevel, HashSet<object> visitedObjects)
    {
        var dictionaryType = dictionary.GetType().Name;
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine($"{dictionaryType}:");
        foreach (DictionaryEntry entry in dictionary)
        {
            sb.Append(indentation + "Key = " +
                      PrintToStringInternal(entry.Key, nestingLevel + 1, visitedObjects));
            sb.Append(indentation + "Value = " +
                      PrintToStringInternal(entry.Value, nestingLevel + 1, visitedObjects));
        }

        visitedObjects.Remove(dictionary);
        return sb.ToString();
    }

    private string HandleEnumerable(IEnumerable enumerable, int nestingLevel, HashSet<object> visitedObjects)
    {
        var collectionType = enumerable.GetType().Name;
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine($"{collectionType}:");
        foreach (var element in enumerable)
        {
            sb.Append(indentation + "- " +
                      PrintToStringInternal(element, nestingLevel + 1, visitedObjects));
        }

        visitedObjects.Remove(enumerable);
        return sb.ToString();
    }

    private string HandleComplexObject(object obj, Type type, int nestingLevel, HashSet<object> visitedObjects)
    {
        var indentationBase = new string('\t', nestingLevel + 1);
        var result = new StringBuilder();
        result.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludingTypes.Contains(propertyInfo.PropertyType) ||
                excludingProperties.Contains(propertyInfo.Name))
                continue;

            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(obj);

            if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                result.Append(indentationBase + propertyName + " = " +
                              propertySerializer(propertyValue) + Environment.NewLine);
            }
            else
            {
                result.Append(indentationBase + propertyName + " = " +
                              PrintToStringInternal(propertyValue, nestingLevel + 1, visitedObjects));
            }
        }

        visitedObjects.Remove(obj);

        return result.ToString();
    }
}