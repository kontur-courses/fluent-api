using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> finalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    ];
    private readonly HashSet<Type> excludedTypes = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializationMethods = [];
    private readonly Dictionary<Type, IFormatProvider> typeCultures = [];
    private readonly Dictionary<string, IFormatProvider> propertyCultures = [];
    private readonly Dictionary<string, Func<object, string>> propertySerializationMethods = [];
    private readonly HashSet<string> excludedProperties = [];
    private readonly HashSet<object> processedObjects = [];
    private readonly Dictionary<string, int> propertyLengths = new();
    
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyName = GetPropertyName(memberSelector);
        
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
    }

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

        if (processedObjects.Contains(obj))
        {
            return "Circular Reference" + Environment.NewLine;
        }

        if (TrySerializeFinalType(obj, out var serializedFinalType))
        {
            return serializedFinalType!;
        }

        if (TrySerializeCollection(obj, nestingLevel, out var serializedCollection))
        {
            return serializedCollection!;
        }
        
        return SerializeComplexType(obj, nestingLevel);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyName = GetPropertyName(memberSelector);
        excludedProperties.Add(propertyName);
        return this;
    }

    public void SpecifyTheCulture<TType>(CultureInfo culture)
    {
        typeCultures[typeof(TType)] = culture;
    }
    
    public void SpecifyTheCulture(CultureInfo culture, string propertyName)
    {
        propertyCultures[propertyName] = culture;
    }

    public void AddSerializationMethod<TType>(Func<TType, string> serializationMethod)
    {
        typeSerializationMethods[typeof(TType)] = obj => serializationMethod((TType)obj);
    }

    public void AddSerializationMethod<TType>(Func<TType, string> serializationMethod, string propertyName)
    {
        propertySerializationMethods[propertyName] = obj => serializationMethod((TType)obj);
    }

    public void AddLengthProperty(string propertyName, int trimLength)
    {
        propertyLengths[propertyName] = trimLength;
    }

    private bool TrySerializeFinalType(object obj, out string? serializedFinalType)
    {
        if (!finalTypes.Contains(obj.GetType()))
        {
            serializedFinalType = null;
            return false;
        }
        
        serializedFinalType = obj switch
        {
            IFormattable format when typeCultures.TryGetValue(obj.GetType(), out var formatProvider) =>
                format.ToString(null, formatProvider),
            IFormattable format => 
                format.ToString(null, CultureInfo.InvariantCulture),
            string str => str,
            _ => (string)obj
        };

        serializedFinalType += Environment.NewLine;
        
        return true;
    }

    private bool TrySerializeCollection(object obj, int nestingLevel, out string? serializedCollection)
    {
        if (obj is not IEnumerable collection)
        {
            serializedCollection = null;
            return false;
        }

        var collectionOnString = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        var type = obj.GetType();

        collectionOnString.AppendLine($"{type.Name}:");
        
        foreach (var item in collection)
        {
            collectionOnString.Append(indentation + PrintToString(item, nestingLevel + 1));
        }

        serializedCollection = collectionOnString.ToString();
        return true;
    }

    private string SerializeComplexType(object obj, int nestingLevel)
    {
        processedObjects.Add(obj);
        var serializedString = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        var type = obj.GetType();
        
        serializedString.AppendLine(type.Name);
        
        foreach (var property in type.GetProperties())
        {
            if (excludedTypes.Contains(property.PropertyType) || excludedProperties.Contains(property.Name))
            {
                continue;
            }

            var serializedProperty = SerializeProperty(property, obj, nestingLevel + 1);
            serializedString.Append(indentation + property.Name + " = " + serializedProperty);
        }

        return serializedString.ToString();
    }

    private string SerializeProperty(PropertyInfo property, object obj, int nestingLevel)
    {
        var propertyValue = property.GetValue(obj);

        if (typeSerializationMethods.TryGetValue(property.PropertyType, out var serializationType))
        {
            return serializationType(propertyValue!) + Environment.NewLine;
        }

        if (propertySerializationMethods.TryGetValue(property.Name, out var serializationProperty))
        {
            return serializationProperty(propertyValue!) + Environment.NewLine;
        }

        if (propertyLengths.TryGetValue(property.Name, out var length))
        {
            return ((string)propertyValue!)[..length] + Environment.NewLine;
        }

        if (propertyCultures.TryGetValue(property.Name, out var formatProvider) && propertyValue is IFormattable format)
        {
            return format.ToString(null, formatProvider) + Environment.NewLine;
        }

        return PrintToString(propertyValue, nestingLevel);
    }
    
    private static string GetPropertyName<T>(Expression<Func<TOwner, T>> property)
    {
        if (property.Body is MemberExpression member)
        {
            return member.Member.Name;
        }
        
        throw new ArgumentException("Expression is not property.");
    }
}