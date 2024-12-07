using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private const int MAX_RECURSION = 1;
    private int currentRecursion = 0;
    internal readonly SerializeConfig<TOwner> config = new();

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj.GetType(), null, obj, 0);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        config.ExcludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        config.AddExcludedPropertyFromExpression(propSelector);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propSelector);
    }

    private bool IsNameOrTypeExcluded(Type type, string name) => config.ExcludedTypes.Contains(type)
                                                                   || config.ExcludedProperties.Contains(name);

    private string PrintToString(Type type, string? name, object obj, int nestingLevel)
    {
        if (obj is TOwner && currentRecursion++ > MAX_RECURSION)
            return $"Object with type {obj.GetType()}";

        if (obj is null) 
            return "null" + Environment.NewLine;

        if (name != null && config.PropertySerializers.TryGetValue(name, out var serializer))
            return serializer.DynamicInvoke(obj).ToString() + Environment.NewLine;

        if (config.TypeSerializers.TryGetValue(type, out var typeSerializer))
            return typeSerializer.DynamicInvoke(obj).ToString() + Environment.NewLine;

        if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
        {
            string result;

            if (config.PropertyCultures.TryGetValue(name, out var nameCulture))
                result = (obj as IFormattable).ToString(null, nameCulture);
            else if (config.TypeCultures.TryGetValue(type, out var typeCulture))
                result = (obj as IFormattable).ToString(null, typeCulture);
            else
                result = obj.ToString();

            if (config.StringPropertiesMaxLen.TryGetValue(name, out var maxLen))
                result = (obj as string)[..maxLen];
            else if (config.StringMaxLen != null && obj is string)
                result = (obj as string)[..config.StringMaxLen.Value];

            return result + Environment.NewLine;
        }

        if (obj is IEnumerable)
            return PrintIEnumerable(obj as IEnumerable, nestingLevel);

        return PrintPropertiesOfType(type, nestingLevel, obj);
    }

    private string PrintIEnumerable(IEnumerable obj, int nestingLevel)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(Environment.NewLine);
        var index = 0;
        foreach (var child in obj)
        {
            sb.Append('\t');
            sb.Append(PrintToString(child.GetType(), $"Element {index++}", child, nestingLevel + 1));
        }
        sb.Append(']');
        return sb.ToString();
    }

    private string PrintPropertiesOfType(Type type, int nestingLevel, object obj)
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
        return sb.ToString();
    }
}