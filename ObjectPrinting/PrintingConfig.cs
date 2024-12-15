using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly Type[] primitiveTypes =
    {
        typeof(int),
        typeof(double),
        typeof(float),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Guid)
    };

    private readonly HashSet<Type> excludingTypes = new();
    private readonly HashSet<PropertyInfo> excludingProperties = new();

    public Dictionary<Type, Func<object, string>> TypeSerializers { get; } = new();
    public Dictionary<Type, CultureInfo> Cultures { get; } = new();
    public Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; } = new();
    public Dictionary<PropertyInfo, int> PropertiesMaxLength { get; } = new();

    public PrintingConfig<TOwner> Excluding<TPropertyType>()
    {
        excludingTypes.Add(typeof(TPropertyType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
    {
        excludingProperties.Add(GetProperty(memberSelector));
        return this;
    }

    public TypePrintingConfig<TOwner, TPropertyType> For<TPropertyType>() => new(this);

    public PropertyPrintingConfig<TOwner, TProperty> For<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TProperty>(this, GetProperty(memberSelector));
    } 

    public string PrintToString(TOwner? obj)
    {
        return Serialize(obj, 0, new Dictionary<object?, int>());
    }

    private string Serialize(object? obj, int nestingLevel, Dictionary<object?, int> parsedObjects)
    {
        if (obj is null)
        {
            return "null";
        }

        if (primitiveTypes.Contains(obj.GetType()))
        {
            return obj + Environment.NewLine;
        }

        if (parsedObjects.TryGetValue(obj, out var level))
        {
            return $"<circular reference to {obj.GetType().Name} at level {level}>";
        }

        parsedObjects[obj] = nestingLevel;

        return obj switch
        {
            IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel, parsedObjects),
            IEnumerable collection => SerializeCollection(collection, nestingLevel, parsedObjects),
            _ => SerializeProperties(obj, nestingLevel, parsedObjects)
        };
    }

    private string SerializeDictionary(IDictionary dictionary, int nestingLevel, Dictionary<object?, int> parsedObjects)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder("Dictionary {\n");

        foreach (DictionaryEntry kvp in dictionary)
        {
            var key = Serialize(kvp.Key, nestingLevel + 1, parsedObjects).Trim();
            var value = Serialize(kvp.Value, nestingLevel + 1, parsedObjects).Trim();
            sb.AppendLine($"{indentation}{key} : {value}");
        }

        sb.AppendLine(new string('\t', nestingLevel) + "}");
        return sb.ToString();
    }

    private string SerializeCollection(IEnumerable collection, int nestingLevel, Dictionary<object?, int> parsedObjects)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder("Collection [\n");

        foreach (var element in collection)
        {
            var value = Serialize(element, nestingLevel + 1, parsedObjects).Trim();
            sb.AppendLine($"{indentation}{value}");
        }

        sb.AppendLine(new string('\t', nestingLevel) + "]");
        return sb.ToString();
    }

    private string SerializeProperties(object? obj, int nestingLevel, Dictionary<object?, int> parsedObjects)
    {
        var type = obj.GetType();
        var sb = new StringBuilder($"{type.Name} {{\n");
        var indentation = new string('\t', nestingLevel + 1);

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (excludingProperties.Contains(propertyInfo) 
                || excludingTypes.Contains(propertyInfo.PropertyType))
                continue;

            var serializedValue = SerializeProperty(propertyInfo, obj, nestingLevel + 1, parsedObjects);
            sb.AppendLine($"{indentation}{propertyInfo.Name} = {serializedValue}");
        }

        sb.AppendLine(new string('\t', nestingLevel) + "}");
        return sb.ToString();
    }

    private string SerializeProperty(PropertyInfo propertyInfo, object? obj, int nestingLevel, Dictionary<object?, int> parsedObjects)
    {
        var propertyValue = propertyInfo.GetValue(obj);

        if (PropertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
        {
            return propertySerializer(propertyValue);
        }

        if (TypeSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
        {
            return typeSerializer(propertyValue);
        }

        if (PropertiesMaxLength.TryGetValue(propertyInfo, out var maxLength) && propertyValue is string str)
        {
            return str[..Math.Min(str.Length, maxLength)];
        }

        return Cultures.TryGetValue(propertyInfo.PropertyType, out var culture)
            ? Convert.ToString(propertyValue, culture) ?? "null"
            : Serialize(propertyValue, nestingLevel, parsedObjects);
    }

    private static PropertyInfo GetProperty<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must refer to a property.");
        }

        return memberExpression.Member as PropertyInfo ?? throw new ArgumentException("Expression must refer to a property.");
    }
}