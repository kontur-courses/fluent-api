using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System;
using System.Collections;

public class ObjectSerializer<TOwner>
{
    private readonly PrintingConfigStorage _config;
    private readonly HashSet<object> _processedObjects = new();
    private const char tab = '\t';

    public ObjectSerializer(PrintingConfigStorage config)
    {
        _config = config;
    }

    public string Serialize(TOwner obj)
    {
        _processedObjects.Clear();
        return SerializeObject(obj, 0);
    }

    private string? SerializeObject(object? obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        if (obj.GetType().IsValueType || obj is string)
        {
            return TrySerializeFinalType(obj, out var serializedFinalType)
                ? serializedFinalType
                : obj.ToString() + Environment.NewLine;
        }

        if (_processedObjects.Contains(obj))
            return "Circular Reference" + Environment.NewLine;

        _processedObjects.Add(obj);

        if (TrySerializeFinalType(obj, out var serializedFinal))
            return serializedFinal!;

        if (TrySerializeCollection(obj, nestingLevel, out var serializedCollection))
            return serializedCollection!;

        return SerializeComplexType(obj, nestingLevel);
    }


    private bool TrySerializeFinalType(object obj, out string? serializedFinalType)
    {
        var type = obj.GetType();

        if (type.IsPrimitive || obj is string || obj is DateTime || obj is TimeSpan || obj is Guid)
        {
            serializedFinalType = obj switch
            {
                IFormattable formattable when _config.TypeCultures.TryGetValue(type, out var culture) =>
                    formattable.ToString(null, culture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => obj.ToString() ?? ""
            };

            serializedFinalType += Environment.NewLine;
            return true;
        }

        serializedFinalType = null;
        return false;
    }

    private bool TrySerializeCollection(object obj, int nestingLevel, out string? serializedCollection)
    {
        if (obj is IDictionary dictionary)
        {
            serializedCollection = SerializeDictionary(dictionary, nestingLevel);
            return true;
        }

        if (obj is not IEnumerable collection)
        {
            serializedCollection = null;
            return false;
        }

        var builder = new StringBuilder();
        var indentation = new string(tab, nestingLevel + 1);

        builder.AppendLine(GetCollectionTypeName(obj) + ":");
        foreach (var item in collection)
            builder.Append(indentation + SerializeObject(item, nestingLevel + 1));

        serializedCollection = builder.ToString();
        return true;
    }

    private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
    {
        var builder = new StringBuilder();
        var indentation = new string(tab, nestingLevel + 1);

        builder.AppendLine(GetCollectionTypeName(dictionary) + ":");
        bool isFirstEntry = true;

        foreach (DictionaryEntry entry in dictionary)
        {
            if (!isFirstEntry)
            {
                builder.AppendLine();
            }

            builder.AppendLine(indentation + "KeyValuePair");
            builder.AppendLine(indentation + "\tKey = " + SerializeObject(entry.Key, nestingLevel + 2).TrimEnd());
            builder.Append(indentation + "\tValue = " + SerializeObject(entry.Value, nestingLevel + 2).TrimEnd());

            isFirstEntry = false;
        }

        return builder.ToString();
    }



    private string SerializeComplexType(object obj, int nestingLevel)
    {
        var builder = new StringBuilder();
        var indentation = new string(tab, nestingLevel + 1);
        var type = obj.GetType();

        builder.AppendLine(type.Name);

        foreach (var property in type.GetProperties())
        {
            if (_config.ExcludedTypes.Contains(property.PropertyType) ||
                _config.ExcludedProperties.Contains(property.Name))
                continue;

            var value = SerializeProperty(property, obj, nestingLevel);
            builder.Append(indentation + property.Name + " = " + value);
        }

        return builder.ToString();
    }

    private string SerializeProperty(PropertyInfo property, object obj, int nestingLevel)
    {
        var value = property.GetValue(obj);

        if (_config.TypeSerializationMethods.TryGetValue(property.PropertyType, out var typeSerializer))
            return typeSerializer(value!) + Environment.NewLine;

        if (_config.PropertySerializationMethods.TryGetValue(property.Name, out var propertySerializer))
            return propertySerializer(value!) + Environment.NewLine;

        if (_config.PropertyLengths.TryGetValue(property.Name, out var length))
        {
            var stringValue = value?.ToString() ?? "";
            return stringValue.Substring(0, Math.Min(length, stringValue.Length)) + Environment.NewLine;
        }

        if (_config.PropertyCultures.TryGetValue(property.Name, out var culture) && value is IFormattable formattable)
            return formattable.ToString(null, culture) + Environment.NewLine;

        return SerializeObject(value, nestingLevel);
    }

    static string GetCollectionTypeName(object obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        var type = obj.GetType();

        if (type.IsGenericType)
        {
            return type.GetGenericTypeDefinition().Name.Split('`')[0];
        }
        else if (type.IsArray)
        {
            return $"{type.GetElementType().Name}[]";
        }
        else
        {
            return type.Name;
        }
    }
}

