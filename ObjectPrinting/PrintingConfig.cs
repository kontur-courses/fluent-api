using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> finalTypes =
    [
        typeof(bool), typeof(byte), typeof(int), typeof(double), typeof(float), typeof(char), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    ];

    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<MemberInfo> excludedProperties = [];
    private readonly HashSet<object> processedObjects = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Type, IFormatProvider> typeCulture = new();
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<MemberInfo, Func<object, string>> propertySerializers = new();
    private int? maxStringLength;

    public PrintingConfig<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> propertyExpression)
    {
        var propertyName = GetPropertyInfo(propertyExpression);
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
        var propertyName = GetPropertyInfo(propertyExpression);
        propertySerializers[propertyName] = obj => serializer((T)obj);
        return this;
    }

    public PrintingConfig<TOwner> SetCulture<T>(IFormatProvider culture) where T : IFormattable
    {
        typeCulture[typeof(T)] = culture;
        return this;
    }

    public PrintingConfig<TOwner> TrimStringsToLength(int maxLength)
    {
        maxStringLength = maxLength;
        return this;
    }

    private static MemberInfo GetPropertyInfo<T>(Expression<Func<TOwner, T>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression member)
        {
            throw new ArgumentException("Expression must be a property access.", nameof(propertyExpression));
        }

        return member.Member;
    }

    public string PrintToString(TOwner obj) => PrintToString(obj, 0);

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
        {
            return AppendNewLine("null");
        }

        if (processedObjects.Contains(obj))
        {
            return AppendNewLine("[Circular Reference]");
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
                text.Append(pad);
                text.Append(PrintToString(item, nestingLevel + 1));
            }

            collectionResult = text.ToString();
            return true;
        }

        collectionResult = null;
        return false;
    }

    private bool TrySerializeFinalType(object obj, out string result)
    {
        var type = obj.GetType();
        if (finalTypes.Contains(type))
        {
            result = (obj switch
            {
                IFormattable formattable when typeCulture.ContainsKey(type) => formattable.ToString(null, typeCulture[type]),
                string str => Trim(str),
                _ => obj.ToString()
            })!;

            result = AppendNewLine(result);
            return true;
        }

        result = null;
        return false;
    }

    private string Trim(string str)
    {
        if (str.Length > maxStringLength)
        {
            return str[..Math.Min(str.Length, maxStringLength.Value)];
        }
        return str;
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
            if (excludedTypes.Contains(property.PropertyType) || excludedProperties.Contains(property))
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
        if (propertySerializers.TryGetValue(property, out var propertySerializer))
            return AppendNewLine(propertySerializer(value));

        if (typeSerializers.TryGetValue(property.PropertyType, out var typeSerializer))
            return AppendNewLine(typeSerializer(value));

        return PrintToString(value, nestingLevel);
    }

    private static string AppendNewLine(ReadOnlySpan<char> text) => string.Concat(text, Environment.NewLine);
}