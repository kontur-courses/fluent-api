using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> finalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    ];

    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<string> excludedProperties = [];
    private readonly HashSet<object> processedObjects = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<string, Func<object, string>> propertySerializers = new();
    private IFormatProvider numberCulture = CultureInfo.InvariantCulture;
    private int? maxStringLength;

    public PrintingConfig<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);
        excludedProperties.Add(propertyName);
        return this;
    }

    public PrintingConfig<TOwner> SetCustomSerialization<T>(Func<T, string> serializer)
    {
        typeSerializers[typeof(T)] = obj => serializer((T)obj);
        return this;
    }

    public PrintingConfig<TOwner> SetCustomSerialization<T>(Expression<Func<TOwner, T>> propertyExpression,
        Func<T, string> serializer)
    {
        var propertyName = GetPropertyName(propertyExpression);
        propertySerializers[propertyName] = obj => serializer((T)obj);
        return this;
    }

    public PrintingConfig<TOwner> SetCulture(IFormatProvider culture)
    {
        numberCulture = culture;
        return this;
    }

    public PrintingConfig<TOwner> TrimStringsToLength(int maxLength)
    {
        maxStringLength = maxLength;
        return this;
    }

    private static string GetPropertyName<T>(Expression<Func<TOwner, T>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression member)
        {
            throw new ArgumentException("Expression must be a property access.", nameof(propertyExpression));
        }

        var propertyName = member.Member.Name;
        return propertyName;
    }

    public string PrintToString(TOwner obj) => PrintToString(obj, 0);

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
        {
            return "null" + Environment.NewLine;
        }

        if (processedObjects.Contains(obj))
        {
            return "[Circular Reference]" + Environment.NewLine;
        }

        if (TrySerializeFinalType(obj, out var result))
        {
            return result;
        }

        if (TrySerializeCollection(obj, nestingLevel, out var collectionResult))
        {
            return collectionResult;
        }

        return SerializeComplexType(obj, nestingLevel);
    }

    private bool TrySerializeCollection(object obj, int nestingLevel, out string collectionResult)
    {
        if (obj is IEnumerable enumerable)
        {
            var text = new StringBuilder();
            var pad = new string('\t', nestingLevel + 1);

            text.AppendLine($"{obj.GetType().Name}:");
            foreach (var item in enumerable)
            {
                text.Append(pad + PrintToString(item, nestingLevel + 1));
            }

            collectionResult = text.ToString();
            return true;
        }

        collectionResult = null;
        return false;
    }

    private bool TrySerializeFinalType(object obj, out string result)
    {
        if (finalTypes.Contains(obj.GetType()))
        {
            result = obj switch
            {
                IFormattable formattable => formattable.ToString(null, numberCulture) + Environment.NewLine,
                string str when maxStringLength.HasValue => string.Concat(
                    str.AsSpan(0, Math.Min(str.Length, maxStringLength.Value)), Environment.NewLine),
                _ => obj + Environment.NewLine
            };
            return true;
        }

        result = null;
        return false;
    }

    private string SerializeComplexType(object obj, int nestingLevel)
    {
        processedObjects.Add(obj);
        var pad = new string('\t', nestingLevel + 1);
        var text = new StringBuilder();
        var type = obj.GetType();
        text.AppendLine(type.Name);

        foreach (var property in type.GetProperties())
        {
            if (excludedTypes.Contains(property.PropertyType) || excludedProperties.Contains(property.Name))
            {
                continue;
            }

            var value = property.GetValue(obj);
            var serializedValue = SerializeProperty(property, value, nestingLevel + 1);
            text.Append(pad + $"{property.Name} = {serializedValue}");
        }

        return text.ToString();
    }

    private string SerializeProperty(PropertyInfo property, object value, int nestingLevel)
    {
        if (propertySerializers.TryGetValue(property.Name, out var propertySerializer))
            return propertySerializer(value) + Environment.NewLine;

        if (typeSerializers.TryGetValue(property.PropertyType, out var typeSerializer))
            return typeSerializer(value) + Environment.NewLine;

        return PrintToString(value, nestingLevel);
    }
}