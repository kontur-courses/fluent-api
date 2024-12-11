using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    public int MaxNestingLevel { get; set; } = 5;
    private readonly HashSet<Type> excludeTypes = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<string> excludeProperties = [];
    private readonly Dictionary<Type, CultureInfo> cultureToFormattable = [];
    private readonly Dictionary<Type, Delegate> typeSerializers = [];
    private readonly Dictionary<string, Delegate> propertySerializers = [];
    private readonly Dictionary<string, int> propertyTrim = [];

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        excludeTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
    {
        excludeProperties.Add(GetPropertyMemberInfo(propertySelector).Name);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> PrintSettings<TPropType>()
    {
        return new(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> PrintSettings<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return new(this, GetPropertyMemberInfo(propertySelector));
    }

    public PrintingConfig<TOwner> UseCulture<TPropType>(CultureInfo culture) where TPropType : IFormattable
    {
        cultureToFormattable[typeof(TPropType)] = culture;
        return this;
    }

    public void AddTypeSerializer<TPropType>(Func<TPropType, string> serializeFunc)
    {
        typeSerializers[typeof(TPropType)] = serializeFunc;
    }

    public void AddPropertySerializer<TPropType>(string propertyName, Func<TPropType, string> serializeFunc)
    {
        propertySerializers[propertyName] = serializeFunc;
    }

    public void AddStringPropertyTrim(string propertyName, int length)
    {
        propertyTrim[propertyName] = length;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 1);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj is null)
        {
            return "null";
        }

        var type = obj.GetType();

        if (typeSerializers.TryGetValue(type, out var serializer))
        {
            return serializer.DynamicInvoke(obj)?.ToString()!;
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
        {
            return obj.ToString()!;
        }

        if (obj is IEnumerable enumerable && type != typeof(string))
        {
            return SerializeEnumerable(enumerable, nestingLevel);
        }

        if (nestingLevel > MaxNestingLevel)
        {
            return $"Достигнут максимум глубины рекурсии: {MaxNestingLevel}.";
        }

        var indentation = new string('\t', nestingLevel);
        var stringBuilderResult = new StringBuilder();
        stringBuilderResult.AppendLine($"{type.Name}");

        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludeProperties.Contains(propertyInfo.Name) || excludeTypes.Contains(propertyInfo.PropertyType))
            {
                continue;
            }

            var propertyValue = propertyInfo.GetValue(obj);
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;

            if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                stringBuilderResult.AppendLine(
                    $"{indentation}{propertyName} = {propertySerializer.DynamicInvoke(propertyValue)}");
                continue;
            }

            if (cultureToFormattable.TryGetValue(propertyType, out var culture))
            {
                var propertyValueFormattable = propertyValue as IFormattable;
                stringBuilderResult.AppendLine(
                    $"{indentation}{propertyName} = {propertyValueFormattable!.ToString(null, culture)}");
                continue;
            }

            if (propertyTrim.TryGetValue(propertyName, out var trimLength) && propertyValue is string stringValue)
            {
                stringBuilderResult.AppendLine(
                    $"{indentation}{propertyName} = {stringValue[..Math.Min(stringValue.Length, stringValue.Length - trimLength)]}");
                continue;
            }

            stringBuilderResult.AppendLine(
                $"{indentation}{propertyName} = {PrintToString(propertyValue, nestingLevel + 1)}");
        }

        return stringBuilderResult.ToString();
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
    {
        var objects = enumerable.Cast<object>().ToList();

        if (objects.Count == 0)
        {
            return "{}";
        }

        var indentation = new string('\t', nestingLevel);
        var serializeResult = new StringBuilder();
        serializeResult.AppendLine("{");

        if (enumerable is IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                var keyConvert = ConvertValueIfNotIFormattable(key, nestingLevel);
                var valueConvert = ConvertValueIfNotIFormattable(value, nestingLevel + 1);

                serializeResult.Append($"{indentation}{keyConvert}: {valueConvert}" + Environment.NewLine);
            }
        }
        else
        {
            for (var i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                serializeResult.Append($"{indentation}{PrintToString(obj, nestingLevel + 1)}");
                if (i < objects.Count - 1)
                {
                    serializeResult.Append($"{indentation},");
                    serializeResult.Append(Environment.NewLine);
                }
            }
        }

        serializeResult.AppendLine($"{indentation}}}");

        return serializeResult.ToString();
    }

    private string ConvertValueIfNotIFormattable(object? value, int nestingLevel)
    {
        return value is IFormattable ? PrintToString(value, nestingLevel) : $"\"{PrintToString(value, nestingLevel)}\"";
    }

    private static MemberInfo GetPropertyMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return propertySelector.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member,
            UnaryExpression { Operand: MemberExpression operand } => operand.Member,
            var _ => throw new ArgumentException("Invalid property selector expression")
        };
    }
}