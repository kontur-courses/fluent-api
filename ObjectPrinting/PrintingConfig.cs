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
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<string> excludedProperties = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<Type, CultureInfo> typeCultures = new();
    private readonly Dictionary<string, Func<object, string>> propertySerializers = new();

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() => new(this);

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector) =>
        new(this, memberSelector);

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Expression must be a member expression", nameof(memberSelector));
        excludedProperties.Add(memberExpression.Member.Name);
        return this;
    }

    internal PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner obj) => PrintToString(obj, 0);

    public void AddSerializerForType<TPropType>(Func<TPropType, string> serializer) =>
        typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);

    public void AddSerializerForProperty(string propertyName, Func<object, string> serializer) =>
        propertySerializers[propertyName] = serializer;

    public void AddCultureForType<TPropType>(CultureInfo culture) => typeCultures[typeof(TPropType)] = culture;



    private string PrintToString(object? obj, int nestingLevel, HashSet<object>? visitedObjects = null)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        visitedObjects ??= [];

        if (!visitedObjects.Add(obj))
            return "Cyclic reference!" + Environment.NewLine;

        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        var type = obj.GetType();

        if (typeSerializers.TryGetValue(type, out var serializer))
            return serializer(obj) + Environment.NewLine;

        if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
            return formattable.ToString(null, culture) + Environment.NewLine;

        if (finalTypes.Contains(type))
            return obj + Environment.NewLine;

        return obj switch
        {
            IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel, visitedObjects),
            IEnumerable enumerable => SerializeEnumerable(enumerable, nestingLevel, visitedObjects),
            _ => SerializeObject(obj, nestingLevel, visitedObjects)
        };
    }

    private string SerializeDictionary(IDictionary dictionary, int nestingLevel, HashSet<object> visitedObjects)
    {
        var dictionaryType = dictionary.GetType().Name;
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();

        sb.AppendLine($"{dictionaryType}:");

        foreach (DictionaryEntry entry in dictionary)
        {
            sb.Append(indentation + "Key = " +
                      PrintToString(entry.Key, nestingLevel + 1, visitedObjects));
            sb.Append(indentation + "Value = " +
                      PrintToString(entry.Value, nestingLevel + 1, visitedObjects));
        }

        visitedObjects.Remove(dictionary);
        return sb.ToString();
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel, HashSet<object> visitedObjects)
    {
        var collectionType = enumerable.GetType().Name;
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();

        sb.AppendLine($"{collectionType}:");

        foreach (var element in enumerable)
        {
            sb.Append(indentation + "- " +
                      PrintToString(element, nestingLevel + 1, visitedObjects));
        }

        visitedObjects.Remove(enumerable);
        return sb.ToString();
    }

    private string SerializeObject(object obj, int nestingLevel, HashSet<object> visitedObjects)
    {
        var type = obj.GetType();
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType) ||
                excludedProperties.Contains(propertyInfo.Name))
                continue;

            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(obj);

            if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                sb.Append(indentation + propertyName + " = " +
                          propertySerializer(propertyValue!) + Environment.NewLine);
            }
            else
            {
                sb.Append(indentation + propertyName + " = " +
                          PrintToString(propertyValue, nestingLevel + 1, visitedObjects));
            }
        }

        visitedObjects.Remove(obj);
        return sb.ToString();
    }
}