using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace ObjectPrinting;

public class ObjectPrinterSettings<TOwner>
{
    private readonly HashSet<Type> finalTypes =
    [
        typeof(bool), typeof(byte), typeof(int), typeof(double), typeof(float), typeof(char), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    ];

    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<MemberInfo> excludedProperties = [];
    private readonly HashSet<object> processedObjects = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Type, IFormatProvider> typeCulture = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = [];
    private readonly Dictionary<MemberInfo, Func<object, string>> propertySerializers = [];
    private int? maxStringLength;
    private const int MaxCollectionItems = 100;
    private const char IndentChar = '\t';

    public ObjectPrinterSettings<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public ObjectPrinterSettings<TOwner> Exclude<T>(Expression<Func<TOwner, T>> propertyExpression)
    {
        var propertyName = GetPropertyInfo(propertyExpression);
        excludedProperties.Add(propertyName);
        return this;
    }

    public ObjectPrinterSettings<TOwner> SetCustomSerialization<T>(Func<T, string> serializer)
    {
        typeSerializers[typeof(T)] = obj => serializer((T)obj);
        return this;
    }

    public ObjectPrinterSettings<TOwner> SetCustomSerialization<T>(Expression<Func<TOwner, T>> propertyExpression, Func<T, string> serializer)
    {
        var propertyName = GetPropertyInfo(propertyExpression);
        propertySerializers[propertyName] = obj => serializer((T)obj);
        return this;
    }

    public ObjectPrinterSettings<TOwner> SetCulture<T>(IFormatProvider culture, string? format = null) where T : IFormattable
    {
        typeCulture[typeof(T)] = culture;
        if (!string.IsNullOrEmpty(format))
            typeSerializers[typeof(T)] = obj => ((IFormattable)obj).ToString(format, culture);
        return this;
    }

    public ObjectPrinterSettings<TOwner> TrimStringsToLength(int maxLength)
    {
        maxStringLength = maxLength;
        return this;
    }

    public string? PrintToString(TOwner obj) => PrintToString(obj, 0);

    private string? PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
            return AppendNewLine("null");

        if (processedObjects.Contains(obj))
            return AppendNewLine("[Circular Reference]");

        if (TrySerializeFinalType(obj, out var result))
            return result;

        return TrySerializeCollection(obj, nestingLevel, out var collectionResult) ? collectionResult : SerializeComplexType(obj, nestingLevel);
    }

    private bool TrySerializeCollection(object obj, int nestingLevel, out string? collectionResult)
    {
        if (obj is IEnumerable enumerable)
        {
            var text = new StringBuilder();
            var pad = new string(IndentChar, nestingLevel + 1);
            var count = 0;

            text.AppendLine($"{obj.GetType().Name}:");
            foreach (var item in enumerable)
            {
                if (count++ >= MaxCollectionItems)
                {
                    text.Append($"{pad}... (truncated)");
                    break;
                }

                text.Append($"{pad}{PrintToString(item, nestingLevel + 1)}");
            }

            collectionResult = text.ToString();
            return true;
        }

        collectionResult = null;
        return false;
    }

    private bool TrySerializeFinalType(object obj, out string? result)
    {
        var type = obj.GetType();
        if (finalTypes.Contains(type))
        {
            result = (obj switch
            {
                IFormattable formattable when typeCulture.ContainsKey(type) =>
                    formattable.ToString("G", typeCulture[type]),
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
        if (maxStringLength.HasValue && str.Length > maxStringLength)
            return str[..maxStringLength.Value];
        return str;
    }

    private string SerializeComplexType(object obj, int nestingLevel)
    {
        processedObjects.Add(obj);
        try
        {
            var pad = new string(IndentChar, nestingLevel + 1);
            var text = new StringBuilder();
            var type = obj.GetType();
            text.AppendLine(type.Name);

            foreach (var property in type.GetProperties())
            {
                if (excludedTypes.Contains(property.PropertyType) || excludedProperties.Contains(property))
                    continue;

                var value = property.GetValue(obj);
                var serializedValue = SerializeProperty(property, value, nestingLevel + 1);
                text.Append($"{pad}{property.Name} = {serializedValue}");
            }

            return text.ToString();
        }
        finally
        {
            processedObjects.Remove(obj);
        }
    }

    private string? SerializeProperty(PropertyInfo property, object? value, int nestingLevel)
    {
        try
        {
            if (propertySerializers.TryGetValue(property, out var propertySerializer))
                return AppendNewLine(propertySerializer(value!));

            if (typeSerializers.TryGetValue(property.PropertyType, out var typeSerializer))
                return AppendNewLine(typeSerializer(value!));

            return PrintToString(value, nestingLevel);
        }
        catch (Exception ex)
        {
            return AppendNewLine($"[Error serializing {property.Name}: {ex.Message}]");
        }
    }

    private static MemberInfo GetPropertyInfo<T>(Expression<Func<TOwner, T>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be a property access.", nameof(propertyExpression));
        return member.Member;
    }

    private static string AppendNewLine(string? text) =>
        text != null && text.EndsWith(Environment.NewLine) ? text : text + Environment.NewLine;
}