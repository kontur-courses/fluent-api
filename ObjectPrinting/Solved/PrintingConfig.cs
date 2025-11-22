using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = [];
    private readonly HashSet<string> excludedMembers = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<string, Func<object, string>> memberSerializers = new();
    private readonly Dictionary<Type, CultureInfo> cultures = new();
    private readonly Dictionary<string, int> stringTrimLengths = new();
    private readonly HashSet<object> visitedObjects = [];

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var member = GetMemberInfo(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropType>(this, member.Name);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var member = GetMemberInfo(memberSelector);
        excludedMembers.Add(member.Name);
        return this;
    }

    internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer)
    {
        typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);
    }

    internal void AddMemberSerializer<TPropType>(string memberName, Func<TPropType, string> serializer)
    {
        memberSerializers[memberName] = obj => serializer((TPropType)obj);
    }

    internal void AddCulture<TPropType>(CultureInfo culture)
    {
        cultures[typeof(TPropType)] = culture;
    }

    internal void SetStringTrimLength(string memberName, int maxLength)
    {
        stringTrimLengths[memberName] = maxLength;
    }


    public string PrintToString(TOwner obj)
    {
        visitedObjects.Clear();
        return PrintToString(obj, 0);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null) return "null" + Environment.NewLine;
        
        if (!visitedObjects.Add(obj))
            return $"Cyclic reference detected ({obj.GetType().Name})" + Environment.NewLine;

        try
        {
            var type = obj.GetType();

            if (type == typeof(string))
            {
                var str = (string)obj;
                return "\"" + str + "\"" + Environment.NewLine;
            }
            
            if (obj is IEnumerable enumerable && !IsFinalType(type))
            {
                return PrintCollection(enumerable, nestingLevel);
            }

            if (IsFinalType(type) || typeSerializers.ContainsKey(type))
            {
                return SerializeValue(obj, type) + Environment.NewLine;
            }
            
            return PrintObject(obj, nestingLevel, type);
        }
        finally
        {
            visitedObjects.Remove(obj);
        }
    }

    private static bool IsFinalType(Type type)
    {
        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(bool),
            typeof(char), typeof(byte), typeof(short), typeof(long),
            typeof(decimal)
        };
        return finalTypes.Contains(type) || type.IsEnum || type.IsPrimitive;
    }

    private string? SerializeValue(object value, Type valueType)
    {
        try
        {
            if (typeSerializers.TryGetValue(valueType, out var typeSerializer))
            {
                return typeSerializer(value);
            }
        
            if (cultures.TryGetValue(valueType, out var culture) 
                && value is IFormattable formattable)
                return formattable.ToString(null, culture);
            return value.ToString();
        }
        catch (Exception ex) when (IsSerializationException(ex))
        {
            return $"[Serialization error: {ex.Message}]";
        }
    }
    
    private static bool IsSerializationException(Exception ex)
    {
        return ex is FormatException or 
            InvalidCastException or 
            NullReferenceException or 
            ArgumentOutOfRangeException or
            DivideByZeroException or
            OverflowException or
            NotSupportedException or
            InvalidOperationException;
    }

    private string PrintObject(object obj, int nestingLevel, Type type)
    {
        var indentationLine = new string('\t', nestingLevel + 1);
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(type.Name);
        
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (excludedTypes.Contains(field.FieldType) || excludedMembers.Contains(field.Name))
                continue;
            stringBuilder.Append(indentationLine + field.Name + " = " +
                                 SerializeMember(field.Name, field.GetValue(obj), nestingLevel));
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (excludedTypes.Contains(property.PropertyType) || excludedMembers.Contains(property.Name))
                continue;

            if (property.GetIndexParameters().Length > 0)
                continue;

            stringBuilder.Append(indentationLine + property.Name + " = " +
                                 SerializeMember(property.Name, property.GetValue(obj), nestingLevel));
        }
        return stringBuilder.ToString();
    }
    private string SerializeMember(string memberName, object? value, int nestingLevel)
    {
        if (value == null)
            return "null" + Environment.NewLine;
        
        if (memberSerializers.TryGetValue(memberName, out var memberSerializer))
        {
            return memberSerializer(value) + Environment.NewLine;
        }
        
        if (value is string strValue && stringTrimLengths.TryGetValue(memberName, out var maxLength))
        {
            var trimmed = strValue.Length <= maxLength ? strValue : strValue[..maxLength];
            return "\"" + trimmed + "\"" + Environment.NewLine;
        }

        var valueType = value.GetType();
        if (typeSerializers.TryGetValue(valueType, out var typeSerializer))
        {
            return typeSerializer(value) + Environment.NewLine;
        }
        
        return PrintToString(value, nestingLevel + 1);
    }

    private string PrintCollection(IEnumerable collection, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var indentationLine = new string('\t', nestingLevel + 1);
        stringBuilder.AppendLine(collection.GetType().Name);

        if (collection is IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                stringBuilder.Append(indentationLine + 
                                     $"[{PrintToString(key, nestingLevel + 1).Trim()}] = " +
                                     PrintToString(dictionary[key], nestingLevel + 1));
            }
        }
        else
        {
            var index = 0;
            foreach (var item in collection)
            {
                stringBuilder.Append(indentationLine + $"[{index}] = " + PrintToString(item, nestingLevel + 1));
                index++;
            }
        }
        return stringBuilder.ToString();
    }
    
    private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member;
        }
        throw new ArgumentException("Expression is not a member access", nameof(memberSelector));
    }
}
