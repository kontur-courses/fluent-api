using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class ObjectPrinter
{
    private readonly PrintingConfig config;
    private readonly HashSet<object> processed = [];

    public static PrintingConfigurator<T> For<T>()
    {
        return new PrintingConfigurator<T>();
    }

    public ObjectPrinter(PrintingConfig config)
    {
        this.config = config;
    }

    public string PrintToString<TOwner>(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string? ProcessCollection(ICollection collection, int nestingLevel)
    {
        var tabsCount = new string('\t', nestingLevel + 1);
        var index = 0;

        var sb = new StringBuilder();
        sb.AppendLine("[");
        foreach (var value in collection)
        {
            var result = PrintToString(value, nestingLevel + 1);
            sb.Append($"{tabsCount}[{index}] = {result}");
            index++;
        }

        sb.Append(new string('\t', nestingLevel) + ']');

        return sb.ToString();
    }

    private string ProcessDictionary(IDictionary dictionary, int nestingLevel)
    {
        var tabsCount = new string('\t', nestingLevel + 1);

        var sb = new StringBuilder();
        sb.AppendLine("{");
        foreach (DictionaryEntry entry in dictionary)
        {
            var key = entry.Key;
            var value = entry.Value;
            sb.Append($"{tabsCount}{key} = {PrintToString(value, nestingLevel + 1)}");
        }

        sb.Append(new string('\t', nestingLevel) + '}');
        return sb.ToString();
    }

    private string? TryFinishProperty(object obj, int nestingLevel)
    {
        if (obj == null)
        {
            return "null";
        }

        if (processed.Contains(obj))
        {
            return "cyclic reference";
        }

        if (obj.GetType().IsSimple())
        {
            return obj.ToString();
        }

        if (obj is IDictionary dictionary)
        {
            return ProcessDictionary(dictionary, nestingLevel);
        }

        if (obj is ICollection collection)
        {
            return ProcessCollection(collection, nestingLevel);
        }

        return null;
    }

    private string? ProcessProperty(PropertyInfo propertyInfo, object obj, int nestingLevel)
    {
        foreach (var action in config.Actions.Where(a => a.CanHandle(propertyInfo)))
        {
            var result = action.Handle(propertyInfo.GetValue(obj));
            return result.IsSkipped ? null : result.Value + Environment.NewLine;
        }

        return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        var possibleResult = TryFinishProperty(obj, nestingLevel);
        if (possibleResult != null) return possibleResult + Environment.NewLine;

        processed.Add(obj);

        var tabsCount = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyResult = ProcessProperty(propertyInfo, obj, nestingLevel);
            if (propertyResult is null) continue;
            sb.Append(tabsCount + propertyInfo.Name + " = " + propertyResult);
        }

        return sb.ToString();
    }
}