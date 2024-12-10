using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private HashSet<object> processedObjects = new(ReferenceEqualityComparer.Instance);
    internal readonly SerializeConfig<TOwner> Config = new();

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj?.GetType(), null, obj, 0);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        Config.ExcludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        Config.AddExcludedPropertyFromExpression(propSelector);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propSelector);
    }

    private bool IsNameOrTypeExcluded(Type type, string name) => Config.ExcludedTypes.Contains(type)
                                                                   || Config.ExcludedProperties.Contains(name);

    private string PrintToString(Type? type, string? name, object obj, int nestingLevel)
    {
        if (obj is null)
            return "null" + Environment.NewLine;

        if (processedObjects.Contains(obj))
            return "{ Recursion object }" + Environment.NewLine;

        if (TryPrintObjectsWithCustomSerializer(type, name, obj, out var serializeResult))
            return serializeResult!;

        if (TryPrintFinalTypes(type, name, obj, out var result))
            return result!;

        if (obj is IEnumerable enumerable)
            return PrintIEnumerable(enumerable, nestingLevel);

        processedObjects.Add(obj);
        return PrintPropertiesAndFieldsOfType(type, nestingLevel, obj);
    }

    private bool TryPrintObjectsWithCustomSerializer(
        Type? type, string? name, object obj, out string? result)
    {
        result = null;
        if (name != null && Config.PropertySerializers.TryGetValue(name, out var serializer))
        {
            result = serializer.Invoke(obj) + Environment.NewLine;
            return true;
        }

        if (Config.TypeSerializers.TryGetValue(type, out var typeSerializer))
        {
            result = typeSerializer.Invoke(obj) + Environment.NewLine;
            return true;
        }

        return false;
    }

    private bool TryPrintFinalTypes(Type? type, string? name, object obj, out string? result)
    {
        result = null;
        var finalTypes = new List<Type>
        { typeof(string), typeof(Guid), typeof(DateTime), typeof(TimeSpan) };

        if (!(type.IsPrimitive || finalTypes.Contains(type)))
            return false;

        if (name != null && Config.PropertyCultures.TryGetValue(name, out var nameCulture))
            result = (obj as IFormattable)!.ToString(null, nameCulture);
        else if (Config.TypeCultures.TryGetValue(type, out var typeCulture))
            result = (obj as IFormattable)!.ToString(null, typeCulture);
        else
            result = obj.ToString();

        if (name != null && Config.StringPropertiesMaxLen.TryGetValue(name, out var maxLen))
        {
            result = obj as string;
            if (result!.Length > maxLen)
                result = result[..maxLen] + "...";
        }
        else if (Config.StringMaxLen != null && obj is string)
        {
            result = obj as string;
            if (result!.Length > Config.StringMaxLen.Value)
                result = result[..Config.StringMaxLen.Value] + "...";
        }

        result += Environment.NewLine;

        return true;
    }

    private string PrintIEnumerable(IEnumerable obj, int nestingLevel)
    {
        if (obj is IDictionary dict)
            return PrintIDictionary(dict, nestingLevel);
        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(Environment.NewLine);
        var index = 0;
        foreach (var child in obj)
        {
            sb.Append(identation);
            sb.Append(PrintToString(child.GetType(), $"Element {index++}", child, nestingLevel + 1));
        }
        sb.Append(new string('\t', nestingLevel));
        sb.Append(']');
        sb.Append(Environment.NewLine);
        return sb.ToString();
    }

    private string PrintIDictionary(IDictionary obj, int nestingLevel)
    {
        var sb = new StringBuilder();
        var identation = new string('\t', nestingLevel + 1);
        sb.Append('{');
        sb.Append(Environment.NewLine);
        var index = 0;
        foreach (var key in obj.Keys)
        {
            sb.Append(identation);
            sb.Append($"Key: {PrintToString(key.GetType(), $"Key {index}", key, nestingLevel + 1)}");
            sb.Append(identation);
            sb.Append($"Value: {PrintToString(obj[key]!.GetType(), $"Value {index++}", obj[key]!, nestingLevel + 1)}");
        }
        sb.Append(new string('\t', nestingLevel));
        sb.Append('}');
        sb.Append(Environment.NewLine);
        return sb.ToString();
    }

    private string PrintPropertiesAndFieldsOfType(Type type, int nestingLevel, object obj)
    {
        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            if (IsNameOrTypeExcluded(propertyInfo.PropertyType, propertyInfo.Name))
                continue;
            sb.Append(identation + propertyInfo.Name + " = " +
                      PrintToString(propertyInfo.PropertyType,
                      propertyInfo.Name,
                      propertyInfo.GetValue(obj),
                      nestingLevel + 1));
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (IsNameOrTypeExcluded(field.FieldType, field.Name))
                continue;
            sb.Append(identation + field.Name + " = " +
                      PrintToString(field.FieldType,
                      field.Name,
                      field.GetValue(obj),
                      nestingLevel + 1));
        }

        return sb.ToString();
    }
}