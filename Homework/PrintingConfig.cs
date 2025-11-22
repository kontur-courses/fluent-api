using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Homework;

public class PrintingConfig<TOwner> : IPrintingConfig
{
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var lambda = memberSelector as LambdaExpression;
        var visitor = new LastMemberVisitor();
        visitor.Visit(lambda.Body);
        var memberInfo = visitor.LastMemberExpression?.Member ?? throw new ArgumentException();

        return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var lambda = memberSelector as LambdaExpression;
        var visitor = new LastMemberVisitor();
        visitor.Visit(lambda.Body);
        var memberInfo = visitor.LastMemberExpression?.Member ?? throw new ArgumentException();

        ((IPrintingConfig)this).ExcludeMembers.Add(memberInfo);

        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        ((IPrintingConfig)this).ExcludeTypes.Add(typeof(TPropType));

        return this;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, null, 0, new(new ObjectReferenceEqualityComparer()));
    }

    private string PrintToString(object? obj, MemberInfo? memberInfo, int nestingLevel, Dictionary<object, int> printedObjects)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();
        if (printedObjects.TryGetValue(obj, out var level))
            return $"(cycle with object {type.Name} at level {level}){Environment.NewLine}";

        if (DoesTypeOverrideToString(type))
            return Serialize(obj, memberInfo) + Environment.NewLine;

        if (type.IsClass)
            printedObjects.Add(obj, nestingLevel);

        var result = obj is ICollection collection
            ? PrintCollectionToString(collection, nestingLevel, printedObjects)
            : PrintComplexObjectToString(obj, nestingLevel, printedObjects);

        if (type.IsClass)
            printedObjects.Remove(obj);

        return result;
    }

    private string PrintCollectionToString(ICollection collection, int nestingLevel, Dictionary<object, int> printedObjects)
    {
        var identation = new string('\t', nestingLevel);
        var sb = new StringBuilder();
        sb.Append((nestingLevel > 0 ? Environment.NewLine + identation : "") + '[' + Environment.NewLine);
        foreach (var item in collection)
            sb.Append(identation + '\t' +
                PrintToString(
                    item,
                    null,
                    nestingLevel + 1,
                    printedObjects));

        sb.Append(identation + ']' + Environment.NewLine);

        return sb.ToString();
    }

    private string PrintComplexObjectToString(object obj, int nestingLevel, Dictionary<object, int> printedObjects)
    {
        var type = obj.GetType();
        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        foreach (var nestedMemberInfo in
            GetPublicPropertiesAndFields(type).OrderBy(t => t.Name))
        {
            if (((IPrintingConfig)this).ExcludeMembers.Contains(nestedMemberInfo))
                continue;

            var newObj = GetValueFromPropertyOrField(obj, nestedMemberInfo);

            if (newObj is not null
                && ((IPrintingConfig)this).ExcludeTypes.Contains(newObj.GetType()))
                continue;

            sb.Append(identation + nestedMemberInfo.Name + " = " +
                PrintToString(
                    newObj,
                    nestedMemberInfo,
                    nestingLevel + 1,
                    printedObjects));
        }

        return sb.ToString();
    }

    private IEnumerable<MemberInfo> GetPublicPropertiesAndFields(Type type)
        => type
            .GetProperties()
            .Select(t => (MemberInfo)t)
            .Concat(type.GetFields());

    private object? GetValueFromPropertyOrField(object obj, MemberInfo memberInfo)
    {
        if (memberInfo is FieldInfo fieldInfo)
            return fieldInfo.GetValue(obj);

        return ((PropertyInfo)memberInfo).GetValue(obj);
    }

    private string Serialize(object obj, MemberInfo? memberInfo)
    {
        if (memberInfo is not null
            && ((IPrintingConfig)this).AlternativeMembersSerialization.TryGetValue(memberInfo, out var serializeMember))
            return serializeMember(obj);

        if (((IPrintingConfig)this).AlternativeTypesSerialization.TryGetValue(obj.GetType(), out var serializeType))
            return serializeType(obj);

        string? str;
        if (((IPrintingConfig)this).TypesCultureInfo.TryGetValue(obj.GetType(), out var culture))
            str = (string)obj.GetType().GetMethod("ToString", [typeof(IFormatProvider)]).Invoke(obj, [culture]);
        else
            str = obj.ToString();

        if (memberInfo is not null
            && ((IPrintingConfig)this).StringsTrim.TryGetValue(memberInfo, out var maxLength)
            && str?.Length > maxLength)
            str = str[..maxLength];

        return str ?? "null";
    }

    private bool DoesTypeOverrideToString(Type type)
    {
        var toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
        return toStringMethod?.DeclaringType != typeof(object) &&
               toStringMethod?.DeclaringType == type;
    }

    HashSet<Type> IPrintingConfig.ExcludeTypes { get; } = [];
    HashSet<MemberInfo> IPrintingConfig.ExcludeMembers { get; } = [];
    Dictionary<Type, Func<object, string>> IPrintingConfig.AlternativeTypesSerialization { get; } = [];
    Dictionary<MemberInfo, Func<object, string>> IPrintingConfig.AlternativeMembersSerialization { get; } = [];
    Dictionary<Type, CultureInfo> IPrintingConfig.TypesCultureInfo { get; } = [];
    Dictionary<MemberInfo, int> IPrintingConfig.StringsTrim { get; } = [];
}
