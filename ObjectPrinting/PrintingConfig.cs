using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly List<object> serializedObjects = [];
    private readonly List<Type> finalTypes =
    [
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(bool)
    ];
    private readonly List<Type> excludedTypes = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new Dictionary<Type, Func<object, string>>();
    private readonly List<MemberInfo> excludedMembers = [];
    private readonly Dictionary<MemberInfo, Func<object, string>> memberSerializers = new Dictionary<MemberInfo, Func<object, string>>();

    public string PrintToString(object obj, int nestingLevel = 0, bool isPrintObjName = true)
    {
        if (obj == null)
            return "null\n";
        if (finalTypes.Contains(obj.GetType()))
            return $"{obj}\n";
        if (serializedObjects.Contains(obj))
            return "Looped! Object was not added\n";
        serializedObjects.Add(obj);

        return obj switch
        {
            IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel),
            ICollection enumerable => SerializeCollection(enumerable, nestingLevel),
            _ => SerializeObject(obj, nestingLevel, isPrintObjName)
        };
    }

    private bool IsNotExcluded(MemberInfo memberInfo) =>
        !excludedTypes.Contains(memberInfo.GetMemberType()) && !excludedMembers.Contains(memberInfo);
    
    private IEnumerable<MemberInfo> GetSerializedMembers(Type type) =>
        type.GetMembers()
            .Where(IsMemberInfoFieldOrProperty)
            .Where(IsNotExcluded);
    
    private string SerializeObject(object obj, int nestingLevel, bool isPrintObjName)
    {
        var objType = obj.GetType();
        var objName = isPrintObjName ? $"{GetTabIndent(nestingLevel)}{objType.Name}\n" : "\n";
        var objBuilder = new StringBuilder(objName);
        foreach (var memberInfo in GetSerializedMembers(objType))
            objBuilder.Append($"{GetTabIndent(nestingLevel + 1)}{memberInfo.Name} = {SerializeMember(obj, memberInfo, nestingLevel)}");
        return objBuilder.ToString();
    }

    private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel) =>
        TryGetCustomSerializer(memberInfo, out var serializer)
            ? $"{serializer(memberInfo.GetValue(obj)!)}\n"
            : PrintToString(memberInfo.GetValue(obj)!, nestingLevel + 1, false);
    
    private string SerializeCollection(IEnumerable collection, int nestingLevel)
    {
        var collectionBuilder = new StringBuilder($"{collection.GetType().Name}\n");
        foreach (var item in collection)
            collectionBuilder.Append(PrintToString(item, nestingLevel + 1));
        return collectionBuilder.ToString();
    }

    private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
    {
        var dictionaryBuilder = new StringBuilder($"{dictionary.GetType().Name}\n");
        foreach (var key in dictionary.Keys)
        {
            dictionaryBuilder.Append(GetTabIndent(nestingLevel + 1));
            dictionaryBuilder.Append($"{PrintToString(key, nestingLevel + 1).Trim()} - ");
            dictionaryBuilder.Append($"{PrintToString(dictionary[key]!, 0).Trim()}\n");
        }
        return dictionaryBuilder.ToString();
    }

    public TypePrintingConfig<TOwner, TPropType> SerializeType<TPropType>() =>
        new TypePrintingConfig<TOwner, TPropType>(this, typeSerializers);

    public PropertyPrintingConfig<TOwner, TPropType> SerializeProperty<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var body = (MemberExpression)memberSelector.Body;
        return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSerializers, body.Member);
    }

    private bool TryGetCustomSerializer(MemberInfo memberInfo, out Func<object, string> serializer)
    {
        if (memberSerializers.ContainsKey(memberInfo))
        {
            serializer = memberSerializers[memberInfo];
            return true;
        }
        var memberType = memberInfo.GetMemberType();
        if (typeSerializers.ContainsKey(memberType))
        {
            serializer = typeSerializers[memberType];
            return true;
        }
        serializer = null;
        return false;
    }

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var body = (MemberExpression) memberSelector.Body;
        excludedMembers.Add(body.Member);
        return this;
    }
    
    private static string GetTabIndent(int indent) => new string('\t', indent);

    private static bool IsMemberInfoFieldOrProperty(MemberInfo memberInfo) =>
        memberInfo.MemberType is MemberTypes.Field or MemberTypes.Property;
}