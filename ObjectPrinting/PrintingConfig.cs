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
    private readonly Dictionary<string, Func<object, string>> propertySerializationMethods = [];
    private readonly HashSet<string> excludedProperties = [];
    private readonly HashSet<object> processedObjects = [];
    private int? maxStringLength;
    
    public static string Serialize<T>(T obj)
    {
        return new PrintingConfig<T>().PrintToString(obj);
    }
    
    public static string Serialize<T>(T obj,
        Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    private string PrintToString(TOwner obj)
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

        var serializedFinalType = SerializeFinalType(obj);

        if (serializedFinalType != null)
        {
            return serializedFinalType;
        }

        return SerializeCollection(obj, nestingLevel) ?? SerializeComplexType(obj, nestingLevel);
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

    public PrintingConfig<TOwner> Trim(int trimLength)
    {
        maxStringLength = trimLength;
        return this;
    }

    public PrintingConfig<TOwner> ExcludeProperty<TType>(Expression<Func<TOwner, TType>> property)
    {
        var propertyName = GetPropertyName(property);
        excludedProperties.Add(propertyName);
        return this;
    }

    private string? SerializeFinalType(object obj)
    {
        if (!finalTypes.Contains(obj.GetType()))
        {
            return null;
        }

        return obj switch
        {
            IFormattable format when typeCultures.TryGetValue(obj.GetType(), out var formatProvider) =>
                format.ToString(null, formatProvider) + Environment.NewLine,
            IFormattable format => 
                format.ToString(null, CultureInfo.InvariantCulture) + Environment.NewLine,
            string str when maxStringLength.HasValue && maxStringLength.Value < str.Length => 
                str[..maxStringLength.Value] + Environment.NewLine,
            string str => str + Environment.NewLine,
            _ => obj + Environment.NewLine
        };
    }

    private string? SerializeCollection(object obj, int nestingLevel)
    {
        if (obj is not IEnumerable collection)
        {
            return null;
        }

        var collectionOnString = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);
        var type = obj.GetType();

        collectionOnString.AppendLine($"{type.Name}:");
        
        foreach (var item in collection)
        {
            collectionOnString.Append(indentation + PrintToString(item, nestingLevel + 1));
        }

        return collectionOnString.ToString();
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