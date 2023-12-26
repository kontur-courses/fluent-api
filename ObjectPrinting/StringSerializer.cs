using ObjectPrinting.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public sealed class StringSerializer<TObject> : ISerializer
{
    private readonly Type[] finalTypes =
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    private readonly HashSet<MemberInfo> ignoredProperties = new();
    private readonly HashSet<Type> ignoredTypes = new();
    private readonly Dictionary<MemberInfo, Delegate> propertySerializers = new();
    private readonly Dictionary<Type, Delegate> typeSerializers = new();
    private readonly HashSet<object> visitedObjects = new();

    private int? lineLength;

    public string Serialize(object instance, int nestingLevel = 0)
    {
        var instanceType = instance.GetType();
        var properties = instanceType.GetProperties();

        var stringBuilder = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);

        if (finalTypes.Contains(instanceType))
            return ((IConvertible)instance).ToString(CultureInfo.CurrentCulture);

        if (instance is IEnumerable enumerable)
            return SerializeEnumerable(enumerable, nestingLevel);

        stringBuilder.AppendLine(instanceType.Name);

        var filteredProperties = ExcludeIgnoredProperties(properties);

        visitedObjects.Add(instance);

        SerializeField(instance, nestingLevel, filteredProperties, stringBuilder, indentation);

        return stringBuilder.ToString();
    }

    public StringSerializer<TObject> Ignoring<TPropertyType>()
    {
        ignoredTypes.Add(typeof(TPropertyType));
        return this;
    }

    public StringSerializer<TObject> Ignoring<TProperty>(Expression<Func<TObject, TProperty>> selector)
    {
        ignoredProperties.Add(((MemberExpression)selector.Body).Member);
        return this;
    }

    public StringSerializer<TObject> ChangeSerializingFor<TProperty>(
        Expression<Func<TObject, TProperty>> selector,
        Func<TProperty, string> serializer)
    {
        propertySerializers.TryAdd(((MemberExpression)selector.Body).Member, serializer);
        return this;
    }

    public StringSerializer<TObject> ChangeSerializingFor<TPropertyType>(Func<TPropertyType, string> serializer)
    {
        typeSerializers.TryAdd(typeof(TPropertyType), serializer);
        return this;
    }

    public StringSerializer<TObject> TrimLinesTo(int length)
    {
        lineLength = length;
        return this;
    }

    private void SerializeField(
        object instance,
        int nestingLevel,
        IEnumerable<PropertyInfo> filteredProperties,
        StringBuilder stringBuilder, string indentation)
    {
        foreach (var propertyInfo in filteredProperties)
        {
            var propertyValue = propertyInfo.GetValue(instance);
            var propertyName = propertyInfo.Name;

            if (propertyValue == null)
            {
                AppendSerializedPropertyToBuilder(stringBuilder, indentation, propertyName, "Null");
                continue;
            }

            // Check for circular reference:
            if (visitedObjects.Contains(propertyValue))
                continue;

            var serializer = FindSerializer(propertyInfo);

            var output = serializer != null
                ? serializer.Method.Invoke(serializer.Target, new[] { propertyValue })!.ToString()!
                : Serialize(propertyValue, nestingLevel + 1);

            AppendSerializedPropertyToBuilder(stringBuilder, indentation, propertyName, output);
        }
    }

    private void AppendSerializedPropertyToBuilder(
        StringBuilder stringBuilder,
        string indentation,
        string propertyName,
        string output)
    {
        var stringValue = $"{propertyName} = {output}";
        stringBuilder.Append(indentation + GetTrimmedString(stringValue) + Environment.NewLine);
    }

    private IEnumerable<PropertyInfo> ExcludeIgnoredProperties(IEnumerable<PropertyInfo> properties)
    {
        var filteredProperties = properties
            .Where(property => !ignoredProperties.Contains(property))
            .Where(property => !ignoredTypes.Contains(property.PropertyType));

        return filteredProperties;
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
    {
        const int itemOffset = 4;

        var stringBuilder = new StringBuilder();
        var options = new SerializingOptions
        {
            ElementOffset = new string(' ', itemOffset),
            PreviousIndent = new string('\t', nestingLevel),
            NestingLevel = nestingLevel
        };

        stringBuilder.AppendLine("[");

        if (enumerable is IDictionary dict)
            FillDictionaryBody(dict, stringBuilder, options);
        else
            FillSequenceBody(enumerable, stringBuilder, options);

        stringBuilder.Append($"{options.PreviousIndent}]");

        return stringBuilder.ToString();
    }

    private void FillDictionaryBody(IDictionary dict, StringBuilder stringBuilder, SerializingOptions options)
    {
        foreach (var key in dict.Keys)
        {
            var output = Serialize(dict[key]!, options.NestingLevel);
            stringBuilder.AppendLine($"{options.PreviousIndent}{options.ElementOffset}[{key}] => {output.TrimEnd()},");
        }
    }

    private void FillSequenceBody(IEnumerable enumerable, StringBuilder stringBuilder, SerializingOptions options)
    {
        foreach (var element in enumerable)
        {
            var output = Serialize(element, options.NestingLevel);
            stringBuilder.AppendLine($"{options.PreviousIndent}{options.ElementOffset}{output.TrimEnd()},");
        }
    }

    private Delegate? FindSerializer(PropertyInfo propertyInfo)
    {
        var memberSerializer = propertySerializers.GetValueOrDefault(propertyInfo);
        var typeSerializer = typeSerializers.GetValueOrDefault(propertyInfo.PropertyType);
        return memberSerializer ?? typeSerializer;
    }

    private string GetTrimmedString(string value)
    {
        if (lineLength == null)
            return value;

        return value.Length <= lineLength ? value : value[..lineLength.Value];
    }
}