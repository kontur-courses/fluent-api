using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<Type, CultureInfo> typeCultures = new();
    
    private readonly Dictionary<string, Func<object, string>> propertySerializers = new();
    
    private readonly List<Type> excludingTypes = [];
    private readonly HashSet<string> excludingProperties = [];
    
    private static readonly HashSet<Type> FinalTypes = new()
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };
        
    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludingTypes.Add(typeof(TPropType));
        return this;
    }
    
    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is MemberExpression memberExpression)
        {
            excludingProperties.Add(memberExpression.Member.Name);
        }
        else
        {
            throw new ArgumentException("Expression must be a member access expression", nameof(memberSelector));
        }

        return this;
    }
        
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }
    
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
    }
        
    public void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer)
    {
        typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);
    }

    public void AddPropertySerializer(string propertyName, Func<object, string> serializer)
    {
        propertySerializers[propertyName] = serializer;
    }

    public void AddTypeCulture<TPropType>(CultureInfo culture)
    {
        typeCultures[typeof(TPropType)] = culture;
    }
        
    public string PrintToString(TOwner obj)
    {
        return PrintToStringInternal(obj, 0, []);
    }

    private string PrintToStringInternal(object? obj, int nestingLevel, HashSet<object> visitedObjects)
    {
        if (obj is null)
        {
            return AddNewLine("null");
        }

        if (!visitedObjects.Add(obj))
        {
            return AddNewLine("(cyclic reference detected)");
        }

        var type = obj.GetType();

        if (typeSerializers.TryGetValue(type, out var serializer))
        {
            return AddNewLine(serializer(obj));
        }

        if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
        {
            return AddNewLine(formattable.ToString(null, culture));
        }

        if (FinalTypes.Contains(type) || type.IsEnum)
        {
            return AddNewLine(obj.ToString());
        }

        if (obj is IDictionary dictionary)
        {
            return dictionary.SerializeDictionary(nestingLevel, visitedObjects, PrintToStringInternal);
        }

        if (obj is IEnumerable enumerable)
        {
            return enumerable.SerializeEnumerable(nestingLevel, visitedObjects, PrintToStringInternal);
        }


        return HandleComplexObject(obj, type, nestingLevel, visitedObjects);
    }
    
    private static string AddNewLine(string? input)
    {
        return input + Environment.NewLine;
    }
    
    private static string GetIndentation(int nestingLevel)
    {
        return new string('\t', nestingLevel + 1);
    }

    private string HandleComplexObject(object obj, Type type, int nestingLevel, HashSet<object> visitedObjects)
    {
        var indentation = GetIndentation(nestingLevel);
        var result = new StringBuilder();
        result.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludingTypes.Contains(propertyInfo.PropertyType) ||
                excludingProperties.Contains(propertyInfo.Name))
            {
                continue;
            }

            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(obj);

            if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                result.AppendLine($"{indentation}{propertyName} = {propertySerializer(propertyValue!)}");
            }
            else
            {
                result.Append($"{indentation}{propertyName} = {PrintToStringInternal(propertyValue, nestingLevel + 1, visitedObjects)}");
            }
        }

        visitedObjects.Remove(obj);
        return result.ToString();
    }
}