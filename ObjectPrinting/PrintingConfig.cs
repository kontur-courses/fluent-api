using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;
using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<object> seenObjects = [];
    private readonly HashSet<Type> excludedProperties = [];
    private readonly HashSet<MemberInfo> excludedMembers = [];
    internal readonly Dictionary<Type, Func<object, string>> TypeSerializationMethod = new();
    internal readonly Dictionary<MemberInfo, Func<object, string>> MemberSerializationMethod = new();

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() => new(this);

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var memberInfo = GetMemberInfo(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
    }

    private static MemberInfo? GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
        (memberSelector.Body as MemberExpression)?.Member;

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var memberInfo = GetMemberInfo(memberSelector);
        if (memberInfo is null)
            throw new ArgumentException("Invalid member selector.");
        excludedMembers.Add(memberInfo);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedProperties.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner owner) =>
        PrintToString(owner, 0);

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        if (seenObjects.Contains(obj))
            return "recursive reference" + Environment.NewLine;

        var type = obj.GetType();

        if (type.IsValueType || type == typeof(string))
            return obj + Environment.NewLine;

        var numberOfTabs = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        if (obj is IEnumerable enumerable)
            sb.Append(GetCollectionString(enumerable, numberOfTabs, nestingLevel));
        else
            sb.Append(GetSingleElementString(obj, numberOfTabs, nestingLevel));

        return sb.ToString();
    }

    private string GetCollectionString(IEnumerable enumerable, string numberOfTabs, int nestingLevel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[");
        foreach (var el in enumerable)
        {
            var value = PrintToString(el, nestingLevel + 1);
            sb.Append($"{numberOfTabs}{value}");
        }

        seenObjects.Add(enumerable);
        sb.AppendLine("]");
        return sb.ToString();
    }

    private string GetSingleElementString(object obj, string numberOfTabs, int nestingLevel)
    {
        IEnumerable<MemberInfo?> propertiesAndFields = obj
            .GetType()
            .GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Where(member => member is PropertyInfo or FieldInfo && !IsMemberExcluded(member));
        var sb = new StringBuilder();
        foreach (var memberInfo in propertiesAndFields)
        {
            sb.Append(numberOfTabs + memberInfo?.Name + " = " +
                      PrintToString(GetValue(obj, memberInfo),
                          nestingLevel + 1));
        }

        seenObjects.Add(obj);
        return sb.ToString();
    }

    private bool IsMemberExcluded(MemberInfo member) =>
        excludedProperties.Contains(member.GetMemberType()) ||
        excludedMembers.Contains(member);

    private object? GetValue(object obj, MemberInfo? memberInfo)
    {
        var val = memberInfo.GetValue(obj);
        if (val is null || memberInfo is null) return null;
        var memberType = memberInfo.GetMemberType();
        return MemberSerializationMethod.TryGetValue(memberInfo, out var memberFunc)
            ? memberFunc.Invoke(val)
            : TypeSerializationMethod.TryGetValue(memberType, out var typeFunc)
                ? typeFunc.Invoke(val)
                : val;
    }
}