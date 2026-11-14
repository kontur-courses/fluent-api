using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type[] FinalTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan)
    ];

    private readonly HashSet<Type> _excludedTypes = [];
    private readonly Dictionary<Type, Func<object, string>> _typePrinters = [];

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    public PrintingConfig<TOwner> Exclude<TExcludingType>()
    {
        ExcludeType(typeof(TExcludingType));
        return this;
    }

    public TypePrintingConfig<TOwner, TPrintingType> Printing<TPrintingType>()
    {
        return new TypePrintingConfig<TOwner, TPrintingType>(this);
    }

    internal void ExcludeType(Type excludedType)
    {
        _excludedTypes.Add(excludedType);
    }

    internal void SetTypePrinter(Type type, Func<object, string> printer)
    {
        _typePrinters[type] = printer;
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();
        if (_typePrinters.TryGetValue(type, out var typePrinter))
            return typePrinter(obj);

        if (IsFinalType(type))
            return obj.ToString();

        var indentation = new string('\t', nestingLevel + 1);
        var builder = new StringBuilder(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            if (_excludedTypes.Contains(propertyInfo.PropertyType))
                continue;

            var printedValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
            builder.Append(Environment.NewLine + indentation + propertyInfo.Name + " = " + printedValue);
        }

        return builder.ToString();
    }

    private static bool IsFinalType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || FinalTypes.Contains(type);
    }
}