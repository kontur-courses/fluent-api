#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Solved;

public static class PrintingConfig
{
    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }
}

public class PrintingConfig<TOwner>
{
    private readonly Dictionary<string, Func<object, string>> _alternativePropertiesPrints = new();
    private readonly Dictionary<Type, Func<object, string>> _alternativeTypesPrints = new();
    private readonly List<MemberInfo> _excludedMembers = new();
    private readonly List<MemberInfo> _members;

    public PrintingConfig() : this(typeof(TOwner))
    {
    }

    public PrintingConfig(TOwner obj) : this(obj.GetType())
    {
    }

    public PrintingConfig(Type type)
    {
        _members = MemberInfoHelper.GetAllPropertiesAndFields(type);
    }

    public IReadOnlyDictionary<string, Func<object, string>> AlternativePropertiesPrints =>
        _alternativePropertiesPrints;

    public IReadOnlyDictionary<Type, Func<object, string>> AlternativeTypesPrints => _alternativeTypesPrints;
    public IReadOnlyList<MemberInfo> ExcludedMembers => _excludedMembers;

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var name = MemberInfoHelper.GetMemberNameFromExpression(memberSelector.Body);
        var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
        _alternativePropertiesPrints[name] = propertyConfig.AlternativePrintInvoke;
        return propertyConfig;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        var type = typeof(TPropType);
        var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
        _alternativeTypesPrints[type] = propertyConfig.AlternativePrintInvoke;
        return propertyConfig;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var name = MemberInfoHelper.GetMemberNameFromExpression(memberSelector.Body);
        foreach (var member in _members)
            if (member.Name == name)
                _excludedMembers.Add(member);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        var type = typeof(TPropType);
        foreach (var member in _members)
            if (MemberInfoHelper.GetTypeFromMemberInfo(member) == type)
                _excludedMembers.Add(member);
        return this;
    }
}