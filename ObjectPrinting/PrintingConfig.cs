using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    internal readonly Dictionary<Type, Func<object, string>> TypePrinters = new();
    internal readonly Dictionary<string, Func<object, string>> PropertyPrinters = new();
    internal IFormatProvider Culture = CultureInfo.InvariantCulture;
    internal readonly Dictionary<string, int> MaxLengths = new();
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<string> excludedProperties = [];
    private readonly HashSet<object> visited = [];

    private readonly HashSet<Type> finalTypes =
        [typeof(int), typeof(double), typeof(float), typeof(DateTime), typeof(TimeSpan)];

    public ITypePrintingConfig<TOwner, TType> Printing<TType>() =>
        new TypePrintingConfig<TOwner, TType>(this);

    public IPropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyName = GetPropertyName(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyName = GetPropertyName(memberSelector);
        excludedProperties.Add(propertyName);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner obj) => PrintToString(obj, 0);

    private static string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be a property access.");

        return member.Member.Name;
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null) return "null" + Environment.NewLine;
        var identation = new string('\t', nestingLevel);

        if (!visited.Add(obj))
            return identation + $"Cycle dependency: {obj}" + Environment.NewLine + Environment.NewLine;
        if (finalTypes.Contains(obj.GetType())) return obj + Environment.NewLine;

        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.Append(identation + type.Name);
        if (obj is not string && type.IsAssignableTo(typeof(IEnumerable)))
            return PrintEnumerable((IEnumerable)obj, nestingLevel);

        sb.AppendLine(PrintFieldsToString(obj, nestingLevel));
        sb.AppendLine(PrintPropertiesToString(obj, nestingLevel));

        return sb.ToString();
    }

    private string PrintPropertiesToString(object obj, int nestingLevel)
    {
        var sb = new StringBuilder();
        var type = obj.GetType();
        var identation = new string('\t', nestingLevel + 1);
        foreach (var propertyInfo in type.GetProperties())
        {
            var property = propertyInfo.GetValue(obj);
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;
            var serializedProperty = Serialize(property, propertyType, propertyName, nestingLevel + 1);
            if (serializedProperty != string.Empty)
                sb.AppendLine(identation + serializedProperty);
        }

        return sb.ToString();
    }

    private string PrintFieldsToString(object obj, int nestingLevel)
    {
        var sb = new StringBuilder();
        var type = obj.GetType();
        var identation = new string('\t', nestingLevel + 1);
        foreach (var fieldInfo in type.GetFields())
        {
            var field = fieldInfo.GetValue(obj);
            var fieldType = fieldInfo.FieldType;
            var fieldName = fieldInfo.Name;
            var serializedField = Serialize(field, fieldType, fieldName, nestingLevel + 1);
            if (serializedField != string.Empty)
                sb.Append(identation + serializedField);
        }

        return sb.ToString();
    }

    private string Serialize<T>(T valueToSerialize, Type type, string name, int level)
    {
        if (excludedTypes.Contains(type) || excludedProperties.Contains(name)) return string.Empty;
        if (valueToSerialize is null) return name + " = " + "null";

        var value = "";
        if (type.IsAssignableTo(typeof(IFormattable)))
            value = ((IFormattable)valueToSerialize).ToString(null, Culture);
        if (PropertyPrinters.TryGetValue(name, out var propertyPrinter))
            value = propertyPrinter(valueToSerialize);
        if (TypePrinters.TryGetValue(type, out var typePrinter))
            value = typePrinter(valueToSerialize);
        if (type.IsAssignableTo(typeof(IEnumerable)))
            value = PrintEnumerable((IEnumerable)valueToSerialize, level);
        if (type.IsAssignableTo(typeof(IDictionary)))
            value = PrintDictionary((IDictionary)valueToSerialize, level);
        if (valueToSerialize is string stringValue && MaxLengths.TryGetValue(name, out var maxLength))
            value = stringValue[..Math.Min(value.Length, maxLength)];
        if (value == "") value = PrintToString(valueToSerialize, level + 1);
        if (value.Length > 0) return name + " = " + value;
        return string.Empty;
    }

    private string PrintEnumerable(IEnumerable? enumerable, int nestingLevel)
    {
        if (enumerable is null) return "null";
        if (enumerable is string) return enumerable.ToString() ?? string.Empty;
        if (finalTypes.Contains(GetElementType(enumerable)))
            return $"[{string.Join(", ", enumerable.Cast<object>().Select(item => item.ToString() ?? string.Empty))}]";
        var sb = new StringBuilder();
        sb.AppendLine("[");
        foreach (var item in enumerable.Cast<object>())
        {
            var serialized = PrintToString(item, nestingLevel + 1);
            sb.Append(serialized.Remove(serialized.Length - 1));
        }

        sb.Append(new string('\t', nestingLevel) + "]");
        return sb.ToString();
    }

    private string PrintDictionary(IDictionary? dictionary, int nestingLevel)
    {
        if (dictionary is null) return "null";
        if (dictionary.Count == 0) return "{}";
        var sb = new StringBuilder();
        var identation = new string('\t', nestingLevel + 1);
        sb.AppendLine("{");
        foreach (var key in dictionary.Keys)
            sb.AppendLine(identation + key + ": " + dictionary[key]);
        sb.Append(new string('\t', nestingLevel) + "}");
        return sb.ToString();
    }
    
    private Type GetElementType(IEnumerable? enumerable)
    {
        if (enumerable is null) return typeof(object);
        var enumerableType = enumerable.GetType()
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableType?.GetGenericArguments().FirstOrDefault() ?? typeof(object);
    }
}