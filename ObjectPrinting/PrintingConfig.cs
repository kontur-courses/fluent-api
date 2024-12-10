using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Configurations;
using ObjectPrinting.Configurations.Interfaces;
using ObjectPrinting.Constants;
using ObjectPrinting.Helpers;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<object> visitedObjects = [];
    private readonly HashSet<Type> excludeTypes = [];
    private readonly HashSet<PropertyInfo> excludeProperties = [];
    private readonly Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>> propertyPrintingConfigs = new();
    private readonly Dictionary<Type, ITypePrintingConfig<TOwner>> typePrintingConfigs = new();
    private CultureInfo commonCulture = CultureInfo.CurrentCulture;

    public PrintingConfig<TOwner> UsingCommonCulture(CultureInfo culture)
    {
        commonCulture = culture;
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludeTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = ReflectionHelper.GetPropertyInfoFromExpression(memberSelector);
        excludeProperties.Add(propertyInfo);
        return this;
    }

    public TypePrintingConfig<TOwner, TType> Printing<TType>()
    {
        var type = typeof(TType);
        if (typePrintingConfigs.TryGetValue(type, out var config)
            && config is TypePrintingConfig<TOwner, TType> printingConfig)
        {
            return printingConfig;
        }

        var typeConfig = new TypePrintingConfig<TOwner, TType>(this);
        typePrintingConfigs[type] = typeConfig;
        return typeConfig;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = ReflectionHelper.GetPropertyInfoFromExpression(memberSelector);
        if (propertyPrintingConfigs.TryGetValue(propertyInfo, out var config)
            && config is PropertyPrintingConfig<TOwner, TPropType> printingConfig)
        {
            return printingConfig;
        }

        var propertyPrintingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
        propertyPrintingConfigs[propertyInfo] = propertyPrintingConfig;
        return propertyPrintingConfig;
    }

    public string PrintToString(TOwner obj) => PrintToString(obj, 0);

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
        if (excludeTypes.Contains(objectType))
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

    private bool TryTypeSerializerPrintString(object obj, Type objectType, out string result)
    {
        result = string.Empty;
        if (!typePrintingConfigs.TryGetValue(objectType, out var config) || config.Serializer is null)
        {
            return false;
        }

        result = config.Serializer(obj);
        return true;
    }

    private CultureInfo GetCultureInfo(Type objectType)
    {
        return typePrintingConfigs.TryGetValue(objectType, out var config) && config.CultureInfo is not null
            ? config.CultureInfo
            : commonCulture;
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
            if (excludeProperties.Contains(propertyInfo)
                || excludeTypes.Contains(propertyInfo.PropertyType))
            {
                continue;
            }

            propertiesStringBuilder.Append($"{indentation}{propertyInfo.Name} = ");
            var propertyValue = propertyInfo.GetValue(obj);
            if (propertyPrintingConfigs.TryGetValue(propertyInfo, out var config)
                && config.Serializer is not null
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