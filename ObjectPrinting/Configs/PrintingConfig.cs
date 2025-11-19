using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly Dictionary<Type, Delegate> typeSerializers;
    private readonly Dictionary<Type, CultureInfo> typeCultures;
    private readonly Dictionary<string, Delegate> propertySerializers;
    private readonly HashSet<string> excludedProperties;
    private readonly HashSet<Type> excludedTypes;
    private readonly HashSet<object> visitedObjects = [];
    
    private readonly Type[] finalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(decimal),
        typeof(long), typeof(short), typeof(byte), typeof(bool),
        typeof(char), typeof(sbyte), typeof(ushort), typeof(uint),
        typeof(ulong)
    ];
    
    private int nestingLevel;
    
    public PrintingConfig()
    {
        typeSerializers = new Dictionary<Type, Delegate>();
        typeCultures = new Dictionary<Type, CultureInfo>();
        propertySerializers = new Dictionary<string, Delegate>();
        excludedProperties = [];
        excludedTypes = [];
        nestingLevel = 50;
    }
    
    private PrintingConfig(
        Dictionary<Type, Delegate> typeSerializers,
        Dictionary<Type, CultureInfo> typeCultures,
        Dictionary<string, Delegate> propertySerializers,
        HashSet<string> excludedProperties,
        HashSet<Type> excludedTypes,
        int maxNestingLevel)
    {
        this.typeSerializers = new Dictionary<Type, Delegate>(typeSerializers);
        this.typeCultures = new Dictionary<Type, CultureInfo>(typeCultures);
        this.propertySerializers = new Dictionary<string, Delegate>(propertySerializers);
        this.excludedProperties = new HashSet<string>(excludedProperties);
        this.excludedTypes = new HashSet<Type>(excludedTypes);
        this.nestingLevel = maxNestingLevel;
    }
    
    private PrintingConfig<TOwner> PrintingConfigWith(
        Dictionary<Type, Delegate> typeSerializers = null,
        Dictionary<Type, CultureInfo> typeCultures = null,
        Dictionary<string, Delegate> propertySerializers = null,
        HashSet<string> excludedProperties = null,
        HashSet<Type> excludedTypes = null,
        int? maxNestingLevel = null)
    {
        return new PrintingConfig<TOwner>(
            typeSerializers ?? this.typeSerializers,
            typeCultures ?? this.typeCultures,
            propertySerializers ?? this.propertySerializers,
            excludedProperties ?? this.excludedProperties,
            excludedTypes ?? this.excludedTypes,
            maxNestingLevel ?? this.nestingLevel);
    }

    public PrintingConfig<TOwner> ExcludeType<T>()
    {
        var newExcludedTypes = new HashSet<Type>(excludedTypes) { typeof(T) };
        return PrintingConfigWith(excludedTypes: newExcludedTypes);
    }

    public TypeSerializingConfig<TOwner, T> SerializeType<T>()
    {
        return new TypeSerializingConfig<TOwner, T>(this);
    }

    public PropertySerializingConfig<TOwner, TProp> SerializeProperty<TProp>(
        Expression<Func<TOwner, TProp>> propertySelector)
    {
        var propertyName = GetPropertyName(propertySelector);
        return new PropertySerializingConfig<TOwner, TProp>(this, propertyName);
    }

    public StringPropertySerializingConfig<TOwner> SerializeProperty(
        Expression<Func<TOwner, string>> propertySelector)
    {
        var propertyName = GetPropertyName(propertySelector);
        return new StringPropertySerializingConfig<TOwner>(this, propertyName);
    }

    public PrintingConfig<TOwner> ExcludeProperty<TProp>(
        Expression<Func<TOwner, TProp>> propertySelector)
    {
        var propertyName = GetPropertyName(propertySelector);
        var newExcludedProperties = new HashSet<string>(excludedProperties) { propertyName };
        return PrintingConfigWith(excludedProperties: newExcludedProperties);
    }

    public string PrintToString(TOwner obj, int? nestingLevel = null)
    {
        if (nestingLevel < 0)
            throw new ArgumentException("Nesting level cannot be negative");
        this.nestingLevel = nestingLevel ?? this.nestingLevel;
        
        visitedObjects.Clear();
        return PrintToStringInternal(obj, 0);
    }

    private string PrintToStringInternal(object obj, int currentnestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;
        
        if (currentnestingLevel >= this.nestingLevel)
            return $"Max nesting level ({this.nestingLevel}) reached" + Environment.NewLine;

        var type = obj.GetType();

        if (visitedObjects.Contains(obj))
            return $"Cyclic reference detected ({type.Name})" + Environment.NewLine;

        visitedObjects.Add(obj);

        try
        {
            if (IsFinalType(type))
                return SerializeValue(obj, type, null) + Environment.NewLine;
            
            if (obj is IEnumerable enumerable && !(obj is string))
                return SerializeCollection(enumerable, currentnestingLevel);

            return SerializeObject(obj, currentnestingLevel, type);
        }
        finally
        {
            visitedObjects.Remove(obj);
        }
    }

    private string SerializeCollection(IEnumerable collection, int nestingLevel)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = collection.GetType();
        
        var collectionTypeName = type.IsGenericType ? GetGenericTypeName(type) : type.Name;
        sb.AppendLine(collectionTypeName);

        var index = 0;
        foreach (var item in collection)
        {
            var serializedItem = SerializeValue(item, 
                item?.GetType() ?? typeof(object), null, nestingLevel + 1);
            
            sb.Append($"{indentation}[{index}] = {serializedItem}");
            index++;
        }

        if (collection is Array || collection is not ICollection coll)
            return sb.ToString();
        
        sb.Append($"{indentation}Count = {coll.Count}" + Environment.NewLine);

        return sb.ToString();
    }

    private string GetGenericTypeName(Type type)
    {
        var genericArgs = type.GetGenericArguments();
        var genericTypeName = type.GetGenericTypeDefinition().Name;
        var cleanName = genericTypeName[..genericTypeName.IndexOf('`')];
        var args = string.Join(", ", genericArgs.Select(t => t.Name));
        return $"{cleanName}`{genericArgs.Length}<{args}>";
    }
    

    private string SerializeObject(object obj, int nestingLevel, Type type)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (ShouldExcludeMember(fieldInfo.FieldType, fieldInfo.Name))
                continue;

            var value = fieldInfo.GetValue(obj);
            var serializedValue = SerializeValue(value, fieldInfo.FieldType,
                fieldInfo.Name, nestingLevel);
            
            sb.Append(indentation + fieldInfo.Name + " = " + serializedValue);
        }

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (propertyInfo.GetIndexParameters().Length > 0)
                continue;

            if (ShouldExcludeMember(propertyInfo.PropertyType, propertyInfo.Name))
                continue;

            var value = propertyInfo.GetValue(obj);
            var serializedValue = SerializeValue(value, propertyInfo.PropertyType, 
                propertyInfo.Name, nestingLevel);
            sb.Append(indentation + propertyInfo.Name + " = " + serializedValue);
        }

        return sb.ToString();
    }

    private string SerializeValue(object value, Type valueType,
        string memberName, int nestingLevel = 0)
    {
        if (value == null)
            return "null" + Environment.NewLine;

        if (memberName != null && propertySerializers.TryGetValue(memberName, out var propertySerializer))
        {
            try
            {
                var result = propertySerializer.DynamicInvoke(value);
                return result?.ToString() + Environment.NewLine;
            }
            catch
            {
                // Fall back to default serialization
            }
        }

        if (typeSerializers.TryGetValue(valueType, out var typeSerializer))
        {
            try
            {
                var result = typeSerializer.DynamicInvoke(value);
                return result?.ToString() + Environment.NewLine;
            }
            catch
            {
                // Fall back to default serialization
            }
        }

        if (typeCultures.TryGetValue(valueType, out var culture) && value is IFormattable formattable)
        {
            return formattable.ToString(null, culture) + Environment.NewLine;
        }

        if (!IsFinalType(valueType))
            return PrintToStringInternal(value, nestingLevel + 1);
        
        if (value is IFormattable formattableDefault)
            return formattableDefault.ToString(null, CultureInfo.InvariantCulture) + Environment.NewLine;

        return value.ToString() + Environment.NewLine;
    }

    private bool ShouldExcludeMember(Type memberType, string memberName)
    {
        return excludedTypes.Contains(memberType) || excludedProperties.Contains(memberName);
    }

    private bool IsFinalType(Type type) => finalTypes.Contains(type) || type.IsEnum;

    private string GetPropertyName<TProp>(
        Expression<Func<TOwner, TProp>> propertySelector)
    {
        switch (propertySelector.Body)
        {
            case MemberExpression memberExpression:
                return memberExpression.Member.Name;
            case UnaryExpression { Operand: MemberExpression unaryMemberExpression }:
                return unaryMemberExpression.Member.Name;
            default:
                throw new ArgumentException("Expression must be a property or field selector");
        }
    }
    
    internal PrintingConfig<TOwner> AddTypeSerializer<TType>(Func<TType, string> serializeFunc)
    {
        var newTypeSerializers = new Dictionary<Type, Delegate>(typeSerializers)
        {
            [typeof(TType)] = serializeFunc
        };
        return PrintingConfigWith(typeSerializers: newTypeSerializers);
    }

    internal PrintingConfig<TOwner> AddTypeCulture<TType>(CultureInfo cultureInfo)
    {
        var newTypeCultures = new Dictionary<Type, CultureInfo>(typeCultures)
        {
            [typeof(TType)] = cultureInfo
        };
        return PrintingConfigWith(typeCultures: newTypeCultures);
    }

    internal PrintingConfig<TOwner> AddPropertySerializer<TProp>(string propertyName,
        Func<TProp, string> serializer)
    {
        var newPropertySerializers = new Dictionary<string, Delegate>(propertySerializers)
        {
            [propertyName] = serializer
        };
        return PrintingConfigWith(propertySerializers: newPropertySerializers);
    }
}