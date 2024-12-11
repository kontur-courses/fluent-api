using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ObjectPrinting.Constants;
using ObjectPrinting.Helpers;

namespace ObjectPrinting;

public class ObjectPrinter<TType>
{
    private readonly HashSet<object> visitedObjects = new(ReferenceEqualityComparer.Instance);
    private readonly PrintingConfig<TType> printingConfig;

    public ObjectPrinter()
    {
        printingConfig = new PrintingConfig<TType>(this);
    }

    public static PrintingConfig<TType> Configure()
    {
        var objectPrinter = new ObjectPrinter<TType>();
        return objectPrinter.GetConfiguration();
    }

    public PrintingConfig<TType> GetConfiguration()
    {
        return printingConfig;
    }

    public string PrintToString(TType obj) => PrintToString(obj, 0);

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj is null)
        {
            return PrintingConfigConstants.NullLine;
        }

        if (visitedObjects.Contains(obj))
        {
            return PrintingConfigConstants.CircularReference;
        }

        var objectType = obj.GetType();
        if (printingConfig.ExcludeTypes.Contains(objectType))
        {
            return string.Empty;
        }

        if (TryTypeSerializerPrintString(obj, objectType, out var serializedString))
        {
            return serializedString;
        }

        var culture = GetCultureInfo(objectType);
        if (TryCorePrintString(obj, objectType, culture, out var finalString))
        {
            return finalString;
        }

        if (objectType.IsClass)
        {
            visitedObjects.Add(obj);
        }

        if (obj is IEnumerable enumerable)
        {
            return SerializeItems(enumerable, nestingLevel);
        }

        var nextNestingLevel = nestingLevel + 1;
        return GetObjectPropertiesString(obj, objectType, nextNestingLevel);
    }

    private CultureInfo GetCultureInfo(Type objectType)
    {
        return printingConfig.TypeCultureConfigs.TryGetValue(objectType, out var cultureConfig)
            ? cultureConfig
            : printingConfig.CommonCulture;
    }

    private bool TryTypeSerializerPrintString(object obj, Type objectType, out string result)
    {
        result = string.Empty;
        if (!printingConfig.TypePrintingConfigs.TryGetValue(objectType, out var config))
        {
            return false;
        }

        result = config.Serializer(obj);
        return true;
    }

    private bool TryCorePrintString(object obj, Type objectType, CultureInfo culture, out string result)
    {
        result = string.Empty;
        if (!PrintingConfigHelper.IsCoreType(objectType))
        {
            return false;
        }

        if (obj is IConvertible convertible)
        {
            result = convertible.ToString(culture);
        }
        else
        {
            result = obj.ToString()!;
        }

        return true;
    }

    private string GetObjectPropertiesString(object obj, Type objectType, int nestingLevel)
    {
        var indentation = GetIndentation(nestingLevel);
        var propertiesStringBuilder = new StringBuilder();
        propertiesStringBuilder.AppendLine(objectType.Name);

        foreach (var propertyInfo in objectType.GetProperties())
        {
            if (printingConfig.ExcludeProperties.Contains(propertyInfo)
                || printingConfig.ExcludeTypes.Contains(propertyInfo.PropertyType))
            {
                continue;
            }

            propertiesStringBuilder.Append($"{indentation}{propertyInfo.Name} = ");
            var propertyValue = propertyInfo.GetValue(obj);
            if (printingConfig.PropertyPrintingConfigs.TryGetValue(propertyInfo, out var config)
                && propertyValue is not null)
            {
                propertiesStringBuilder.AppendLine($"{config.Serializer(propertyValue)}");
            }
            else
            {
                propertiesStringBuilder.AppendLine($"{PrintToString(propertyValue, nestingLevel)}");
            }
        }

        return propertiesStringBuilder.ToString();
    }

    private string SerializeItems(IEnumerable enumerable, int nestingLevel)
    {
        var indentation = GetIndentation(nestingLevel);
        var nextNestingLevel = nestingLevel + 1;
        string returnString;
        if (enumerable is IDictionary dictionary)
        {
            returnString = SerializeDictionary(dictionary, nextNestingLevel);
        }
        else
        {
            returnString = SerializeEnumerable(enumerable, nextNestingLevel);
        }

        return string.Format("{0}{1}[{0}{2}{1}]", PrintingConfigConstants.NewLine, indentation, returnString);
    }

    private string SerializeEnumerable(IEnumerable items, int nestingLevel)
    {
        var enumerableSerializeBuilder = new StringBuilder();
        var indentation = GetIndentation(nestingLevel);
        var nextNestingLevel = nestingLevel + 1;
        foreach (var item in items)
        {
            enumerableSerializeBuilder.AppendLine(
                $"{indentation}{PrintToString(item, nextNestingLevel)}");
        }

        return enumerableSerializeBuilder.ToString();
    }

    private string SerializeDictionary(IDictionary items, int nestingLevel)
    {
        var dictionarySerializeBuilder = new StringBuilder();
        var indentation = GetIndentation(nestingLevel);
        var nextNestingLevel = nestingLevel + 1;
        foreach (DictionaryEntry entry in items)
        {
            dictionarySerializeBuilder.AppendLine(
                $"{indentation}{entry.Key} = {PrintToString(entry.Value, nextNestingLevel)}");
        }

        return dictionarySerializeBuilder.ToString();
    }

    private string GetIndentation(int nestingLevel)
    {
        return new string(PrintingConfigConstants.Tab, nestingLevel);
    }
}