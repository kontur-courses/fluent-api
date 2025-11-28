using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly IReadOnlySet<Type> _typesToExclude;
    private readonly IReadOnlySet<MemberInfo> _membersToExclude;
    private readonly IReadOnlyDictionary<Type, CultureInfo> _cultureSerializers;
    private readonly IReadOnlyDictionary<Type, Delegate> _typeSerializers;
    private readonly IReadOnlyDictionary<MemberInfo, Delegate> _memberSerializers;
    private readonly int _maxNestingLevel;

    public PrintingConfig()
    {
        _typesToExclude = new HashSet<Type>();
        _membersToExclude = new HashSet<MemberInfo>();
        _cultureSerializers = new Dictionary<Type, CultureInfo>();
        _typeSerializers = new Dictionary<Type, Delegate>();
        _memberSerializers = new Dictionary<MemberInfo, Delegate>();
        _maxNestingLevel = 5;
    }

    private PrintingConfig(
        IReadOnlySet<Type> typesToExclude,
        IReadOnlySet<MemberInfo> membersToExclude, 
        IReadOnlyDictionary<Type, CultureInfo> cultureSerializers,
        IReadOnlyDictionary<Type, Delegate> typeSerializers,
        IReadOnlyDictionary<MemberInfo, Delegate> memberSerializers,
        int maxNestingLevel)
    {
        _typesToExclude = typesToExclude;
        _membersToExclude = membersToExclude;
        _cultureSerializers = cultureSerializers;
        _typeSerializers = typeSerializers;
        _memberSerializers = memberSerializers;
        _maxNestingLevel = maxNestingLevel;
    }

    public PrintingConfig<TOwner> SetMaxNestingLevel(int maxNestingLevel)
    {
        if (maxNestingLevel < 0)
            throw new ArgumentException("Max nesting level must be greater than or equal to 0.");

        return new PrintingConfig<TOwner>(
            _typesToExclude,
            _membersToExclude,
            _cultureSerializers,
            _typeSerializers,
            _memberSerializers,
            maxNestingLevel);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        var newTypesToExclude = new HashSet<Type>(_typesToExclude) { typeof(TPropType) };
        return new PrintingConfig<TOwner>(
            newTypesToExclude,
            _membersToExclude,
            _cultureSerializers,
            _typeSerializers,
            _memberSerializers,
            _maxNestingLevel);
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
    {
        var memberInfo = GetMemberInfo(propertySelector);
        ValidateMemberInfo(memberInfo);
        
        var newMembersToExclude = new HashSet<MemberInfo>(_membersToExclude) { memberInfo }; 
        return new PrintingConfig<TOwner>(
            _typesToExclude,
            newMembersToExclude,
            _cultureSerializers,
            _typeSerializers,
            _memberSerializers,
            _maxNestingLevel);
    }

    public IPropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
    {
        var memberInfo = GetMemberInfo(propertySelector);
        ValidateMemberInfo(memberInfo);
    
        return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo); 
    }

    public ITypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new TypePrintingConfig<TOwner, TPropType>(this);
    }
    
    public ITypePrintingConfig<TOwner, TPropType> PrintSettings<TPropType>()
    {
        return Printing<TPropType>();
    }

    public IPropertyPrintingConfig<TOwner, TPropType> PrintPropertySettings<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return Printing(propertySelector);
    }
    
    public PrintingConfig<TOwner> UseCulture<TPropType>(CultureInfo culture) where TPropType : IFormattable
    {
        return SetCulture<TPropType>(culture);
    }

    public PrintingConfig<TOwner> SetCulture<TPropType>(CultureInfo culture) where TPropType : IFormattable
    {
        var newCultureSerializers = new Dictionary<Type, CultureInfo>(_cultureSerializers)
        {
            [typeof(TPropType)] = culture
        };
        
        return new PrintingConfig<TOwner>(
            _typesToExclude,
            _membersToExclude,
            newCultureSerializers,
            _typeSerializers,
            _memberSerializers,
            _maxNestingLevel);
    }

    internal PrintingConfig<TOwner> WithTypeSerializer<TPropType>(Func<TPropType, string> serializeFunc)
    {
        var newTypeSerializers = new Dictionary<Type, Delegate>(_typeSerializers)
        {
            [typeof(TPropType)] = serializeFunc
        };
        
        return new PrintingConfig<TOwner>(
            _typesToExclude,
            _membersToExclude,
            _cultureSerializers,
            newTypeSerializers,
            _memberSerializers,
            _maxNestingLevel);
    }

    internal PrintingConfig<TOwner> WithMemberSerializer<TPropType>(MemberInfo memberInfo, Func<TPropType, string> serializeFunc)
    {
        var newMemberSerializers = new Dictionary<MemberInfo, Delegate>(_memberSerializers)
        {
            [memberInfo] = serializeFunc
        };
        
        return new PrintingConfig<TOwner>(
            _typesToExclude,
            _membersToExclude,
            _cultureSerializers,
            _typeSerializers,
            newMemberSerializers,
            _maxNestingLevel);
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 1, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private string PrintToString(object? obj, int nestingLevel, ISet<object> processedObjects)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();
        
        if ((type.IsPrimitive || type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime)) 
            && _typesToExclude.Contains(type))
            return $"<excluded {type.Name}>";

        if (processedObjects.Contains(obj))
            return $"Cyclic reference detected: {type.Name}";

        if (_typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            var result = typeSerializer.DynamicInvoke(obj) as string;
            return result ?? "null";
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime))
            return type == typeof(string) ? obj.ToString() : FormatWithCulture(obj, type);

        if (nestingLevel > _maxNestingLevel)
            return $"... (max nesting level {_maxNestingLevel} reached)";

        processedObjects.Add(obj);

        try
        {
            if (obj is IEnumerable enumerable && type != typeof(string))
                return SerializeEnumerable(enumerable, nestingLevel, processedObjects);

            return SerializeObject(obj, nestingLevel, processedObjects, type);
        }
        finally
        {
            processedObjects.Remove(obj);
        }
    }
    
    private string FormatWithCulture(object obj, Type type)
    {
        if (_cultureSerializers.TryGetValue(type, out var culture) && obj is IFormattable formattable)
            return formattable.ToString(null, culture);
            
        return obj.ToString() ?? "null";
    }

    private string SerializeObject(object obj, int nestingLevel, ISet<object> processedObjects, Type type)
    {
        var indentation = new string('\t', nestingLevel);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        var members = type.GetProperties().Cast<MemberInfo>()
            .Concat(type.GetFields().Cast<MemberInfo>());

        foreach (var member in members)
        {
            if (ShouldExcludeMember(member))
                continue;

            var value = GetMemberValue(member, obj);
            var serializedValue = SerializeMember(member.Name, value, member.GetMemberType(), 
                nestingLevel, processedObjects, indentation, member); 

            sb.AppendLine(serializedValue);
        }

        return sb.ToString();
    }

    private bool ShouldExcludeMember(MemberInfo member)
    {
        var memberType = member.GetMemberType();
        var shouldExclude = _typesToExclude.Contains(memberType) || 
                            _membersToExclude.Contains(member); 
    
        return shouldExclude;
    }

    private string SerializeMember(string memberName, object? value, Type memberType, 
        int nestingLevel, ISet<object> processedObjects, string indentation, MemberInfo memberInfo) 
    {
        if (_memberSerializers.TryGetValue(memberInfo, out var memberSerializer))
        {
            var result = memberSerializer.DynamicInvoke(value);
            return $"{indentation}{memberName} = {result}";
        }

        if (value != null && _typeSerializers.TryGetValue(memberType, out var typeSerializer))
        {
            var result = typeSerializer.DynamicInvoke(value);
            return $"{indentation}{memberName} = {result}";
        }
        
        if (value is IFormattable formattable && _cultureSerializers.TryGetValue(memberType, out var culture))
            return $"{indentation}{memberName} = {formattable.ToString(null, culture)}";

        return $"{indentation}{memberName} = {PrintToString(value, nestingLevel + 1, processedObjects)}";
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel, ISet<object> processedObjects)
{
    if (enumerable is IDictionary dictionary)
        return SerializeDictionary(dictionary, nestingLevel, processedObjects);

    return SerializeList(enumerable, nestingLevel, processedObjects);
}

    private string SerializeList(IEnumerable enumerable, int nestingLevel, ISet<object> processedObjects)
    {
        var items = new List<object>();
        int totalCount = 0;
        const int maxItems = 100;
        bool hasMoreItems = false;

        foreach (var item in enumerable)
        {
            if (totalCount >= maxItems)
            {
                hasMoreItems = true;
                break;
            }
            
            items.Add(item);
            totalCount++;
        }

        if (items.Count == 0)
            return "[]";

        var indentation = new string('\t', nestingLevel);
        var sb = new StringBuilder();
        sb.AppendLine("[");

        for (int i = 0; i < items.Count; i++)
        {
            var serializedItem = PrintToString(items[i], nestingLevel + 1, processedObjects);
            sb.Append($"{indentation}{serializedItem}");
        
            if (i < items.Count - 1)
                sb.AppendLine(",");
            else
                sb.AppendLine();
        }

        if (hasMoreItems)
            sb.AppendLine($"{indentation}... (showing first {maxItems} items)");

        sb.Append($"{new string('\t', nestingLevel - 1)}]");
        return sb.ToString();
    }

    private string SerializeDictionary(IDictionary dictionary, int nestingLevel, ISet<object> processedObjects)
    {
        var indentation = new string('\t', nestingLevel);
        var sb = new StringBuilder();
        sb.AppendLine("[");

        int count = 0;
        const int maxItems = 100;
        bool hasMoreItems = false;

        foreach (DictionaryEntry entry in dictionary)
        {
            if (count >= maxItems)
            {
                hasMoreItems = true;
                break;
            }

            var serializedKey = PrintToString(entry.Key, nestingLevel + 1, processedObjects);
            var serializedValue = PrintToString(entry.Value, nestingLevel + 1, processedObjects);
            
            sb.AppendLine($"{indentation}{serializedKey}: {serializedValue}");
            count++;
        }

        if (hasMoreItems)
            sb.AppendLine($"{indentation}... (showing first {maxItems} items)");

        sb.Append($"{new string('\t', nestingLevel - 1)}]");
        return count == 0 ? "[]" : sb.ToString();
    }

    private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> expression)
    {
        if (expression.Body is MemberExpression memberExpr)
            return memberExpr.Member;
    
        if (expression.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression operandMemberExpr)
            return operandMemberExpr.Member;
    
        throw new ArgumentException("Expression must be a property or field access");
    }

    private static void ValidateMemberInfo(MemberInfo memberInfo)
    {
        if (memberInfo.MemberType != MemberTypes.Property && memberInfo.MemberType != MemberTypes.Field)
            throw new ArgumentException("Expression must reference a property or field");
    }

    private static object? GetMemberValue(MemberInfo member, object obj)
    {
        if (member is PropertyInfo property)
            return property.GetValue(obj);
    
        if (member is FieldInfo field)
            return field.GetValue(obj);
    
        return null;
    }
}

public static class MemberInfoExtensions
{
    public static Type GetMemberType(this MemberInfo member)
    {
        if (member is PropertyInfo property)
            return property.PropertyType;
    
        if (member is FieldInfo field)
            return field.FieldType;
    
        throw new ArgumentException("Member must be a property or field");
    }
}