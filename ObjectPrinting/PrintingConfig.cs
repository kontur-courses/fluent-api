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
    private readonly HashSet<Type> excludedProperties = [];
    private readonly HashSet<MemberInfo> excludedMembers = [];
    internal readonly Dictionary<Type, Delegate> TypeSerializationMethod = new();
    internal readonly Dictionary<MemberInfo, Delegate> MemberSerializationMethod = new();

    public PrintingConfig(PrintingConfig<TOwner>? parent = null)
    {
        if (parent == null)
            return;

        excludedProperties.AddRange(parent.excludedProperties);
        excludedMembers.AddRange(parent.excludedMembers);
        TypeSerializationMethod.AddRange(parent.TypeSerializationMethod);
        MemberSerializationMethod.AddRange(parent.MemberSerializationMethod);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        var configCopy = new PrintingConfig<TOwner>(this);
        return new PropertyPrintingConfig<TOwner, TPropType>(configCopy);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var configCopy = new PrintingConfig<TOwner>(this);
        var memberInfo = GetMemberInfo(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropType>(configCopy, memberInfo);
    }
    
    private static MemberInfo? GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
        (memberSelector.Body as MemberExpression)?.Member;

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var configClone = new PrintingConfig<TOwner>(this);
        var memberInfo = GetMemberInfo(memberSelector);
        if (memberInfo is null)
            throw new ArgumentException("Invalid member selector.");
        configClone.excludedMembers.Add(memberInfo);
        return configClone;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        var configCopy = new PrintingConfig<TOwner>(this);
        configCopy.excludedProperties.Add(typeof(TPropType));
        return configCopy;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        if (obj.GetType().IsFinal())
            return obj + Environment.NewLine;

        var numberOfTabs = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var memberInfo in type
                     .GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                     .Where(member =>
                         member is PropertyInfo or FieldInfo && !excludedProperties.Contains(member.GetMemberType()!)))
        {
            sb.Append(numberOfTabs + memberInfo.Name + " = " +
                      PrintToString(GetValue(obj, memberInfo),
                          nestingLevel + 1));
        }

        return sb.ToString();
    }

    private object? GetValue(object obj, MemberInfo memberInfo)
    {
        var val = memberInfo.GetValue(obj);
        var memberType = memberInfo.GetMemberType();
        return MemberSerializationMethod.TryGetValue(memberInfo, out var memberFunc)
            ? memberFunc.DynamicInvoke(val)
            : TypeSerializationMethod.TryGetValue(memberType!, out var typeFunc)
            ? typeFunc.DynamicInvoke(val)
            : val;
    }
}