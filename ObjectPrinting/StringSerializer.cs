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
    private readonly Dictionary<Type, CultureInfo> typeCultures = new();
    private readonly Dictionary<Type, Delegate> typeSerializers = new();
    private readonly HashSet<object> visitedObjects = new();

    private int? lineLength;

    string ISerializer.Serialize(object instance, int nestingLevel)
    {
        var instanceType = instance.GetType();
        var properties = instanceType.GetProperties();

        var stringBuilder = new StringBuilder();
        var indentation = new string('\t', nestingLevel + 1);

        if (finalTypes.Contains(instanceType))
        {
            if (instanceType != typeof(string) || lineLength == null)
                return ApplyCultureToString(instance, instanceType);

            return GetTrimmedString(instance);
        }

        if (instance is IEnumerable enumerable)
            return SerializeEnumerable(enumerable, nestingLevel);

        stringBuilder.AppendLine(instanceType.Name);

        var prefilter = properties
            .Where(property => !ignoredProperties.Contains(property))
            .Where(property => !ignoredTypes.Contains(property.PropertyType));

        visitedObjects.Add(instance);

        foreach (var propertyInfo in prefilter)
        {
            var propertyValue = propertyInfo.GetValue(instance);
            var propertyName = propertyInfo.Name;

            if (propertyValue == null)
            {
                stringBuilder.Append($"{indentation}{propertyName} = Null{Environment.NewLine}");
                continue;
            }

            // Check for circular reference:
            if (visitedObjects.Contains(propertyValue))
                continue;

            var serializer = FindSerializer(propertyInfo);

            var output = serializer != null
                ? serializer.Method.Invoke(serializer.Target, new[] { propertyValue })!.ToString()!
                : (this as ISerializer).Serialize(propertyValue, nestingLevel + 1);

            stringBuilder.Append($"{indentation}{propertyName} = {output}{Environment.NewLine}");
        }

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

    public StringSerializer<TObject> SetCultureTo<TPropertyType>(CultureInfo culture)
    {
        typeCultures.TryAdd(typeof(TPropertyType), culture);
        return this;
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
    {
        const int customOffset = 4;

        var stringBuilder = new StringBuilder();

        var previousIndent = new string('\t', nestingLevel);
        var elementOffset = new string(' ', customOffset);

        stringBuilder.AppendLine("[");

        if (enumerable is IDictionary dict) FillDictionaryBody();
        else FillSequenceBody();

        stringBuilder.Append($"{previousIndent}]");

        return stringBuilder.ToString();

        void FillSequenceBody()
        {
            foreach (var element in enumerable)
            {
                var output = (this as ISerializer).Serialize(element, nestingLevel);
                stringBuilder.AppendLine($"{previousIndent}{elementOffset}{output.TrimEnd()},");
            }
        }

        void FillDictionaryBody()
        {
            foreach (var key in dict.Keys)
            {
                var output = (this as ISerializer).Serialize(dict[key]!, nestingLevel);
                stringBuilder.AppendLine($"{previousIndent}{elementOffset}[{key}] => {output.TrimEnd()},");
            }
        }
    }

    private string ApplyCultureToString(object instance, Type instanceType)
    {
        var culture = typeCultures.GetValueOrDefault(instanceType) ?? CultureInfo.CurrentCulture;
        return ((IConvertible)instance).ToString(culture);
    }

    private Delegate? FindSerializer(PropertyInfo propertyInfo)
    {
        var memberSerializer = propertySerializers.GetValueOrDefault(propertyInfo);
        var typeSerializer = typeSerializers.GetValueOrDefault(propertyInfo.PropertyType);
        return memberSerializer ?? typeSerializer;
    }

    private string GetTrimmedString(object instance)
    {
        var stringValue = instance.ToString()!;

        if (lineLength == null)
            return stringValue;

        return stringValue.Length < lineLength ? stringValue : stringValue[..lineLength.Value];
    }
}