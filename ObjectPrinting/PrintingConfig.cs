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

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludeTypes = [];
    private readonly HashSet<PropertyInfo> excludeProperties = [];

    private readonly IDictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>>
        propertyPrintingConfigs = new Dictionary<PropertyInfo, IPropertyPrintingConfig<TOwner>>();

    private readonly IDictionary<Type, ITypePrintingConfig<TOwner>>
        typePrintingConfigs = new Dictionary<Type, ITypePrintingConfig<TOwner>>();

    private readonly HashSet<object> visitedObjects = [];
    
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

    public TypePrintingConfig<TOwner, TType> Printing<TType>()
    {
        var type = typeof(TType);
        if (typePrintingConfigs.TryGetValue(type, out var printing) 
            && printing is TypePrintingConfig<TOwner, TType> config)
        {
            return config;
        }
        var typeConfig = new TypePrintingConfig<TOwner, TType>(this);
        typePrintingConfigs[type] = typeConfig;
        return typeConfig;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = GetPropertyInfoFromExpression(memberSelector);
        excludeProperties.Add(propertyInfo);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyInfo = GetPropertyInfoFromExpression(memberSelector);
        if (propertyPrintingConfigs.TryGetValue(propertyInfo, out var printing) 
            && printing is PropertyPrintingConfig<TOwner, TPropType> config)
        {
            return config;
        }
        var printingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
        propertyPrintingConfigs[propertyInfo] = printingConfig;
        return printingConfig;
    }

    public string PrintToString(TOwner obj) => PrintToString(obj, 0);

    private PropertyInfo GetPropertyInfoFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression expression)
        {
            throw new ArgumentException("Expression must be a member expression", nameof(memberSelector));
        }

        if (expression.Expression == expression)
        {
            throw new ArgumentException("Cannot exclude the entire object. Use a property expression.",
                nameof(memberSelector));
        }

        if (expression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression must be a property expression", nameof(memberSelector));
        }

        return propertyInfo;
    }

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

        if (TryTypeSerializerPrintString(obj, out var serializerString))
        {
            return serializerString;
        }

        var culture = GetCultureInfo(objectType);
        if (TryFinalPrintString(obj, culture, out var finalString))
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
        return GetObjectPropertiesString(obj, nextNestingLevel);
    }

    private bool TryTypeSerializerPrintString(object? obj, out string result)
    {
        result = string.Empty;
        if (obj is null)
        {
            return false;
        }

        var objectType = obj.GetType();
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

    private bool TryFinalPrintString(object? obj, CultureInfo culture, out string result)
    {
        result = string.Empty;
        if (obj is null)
        {
            return false;
        }

        var objectType = obj.GetType();
        if (!PrintingConfigConstants.FinalTypes.Contains(objectType))
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

    private string GetObjectPropertiesString(object obj, int nestingLevel)
    {
        var objectType = obj.GetType();
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
            var propertyValue = propertyInfo.GetValue(obj);
            if (propertyPrintingConfigs.TryGetValue(propertyInfo, out var config)
                && config.Serializer is not null 
                && propertyValue is not null)
            {
                propertiesStringBuilder.AppendLine(
                    $"{indentation}{propertyInfo.Name} = {config.Serializer(propertyValue)}");
            }
            else
            {
                propertiesStringBuilder.AppendLine(
                    $"{indentation}{propertyInfo.Name} = {PrintToString(propertyValue, nestingLevel)}");
            }
        }

        return propertiesStringBuilder.ToString();
    }

    private string SerializeItems(IEnumerable? enumerable, int nestingLevel)
    {
        if (enumerable is null)
        {
            return PrintingConfigConstants.NullLine;
        }
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
        foreach (var itemKey in items.Keys)
        {
            dictionarySerializeBuilder.AppendLine(
                $"{indentation}{itemKey} = {PrintToString(items[itemKey], nextNestingLevel)}");
        }
        return dictionarySerializeBuilder.ToString();
    }

    private string GetIndentation(int nestingLevel)
    {
        return new string('\t', nestingLevel);
    }
}