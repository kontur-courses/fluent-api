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
    private readonly List<MemberInfo> _members = new();

    public PrintingConfig()
    {
        _members.AddRange(typeof(TOwner).GetProperties(BindingFlags.Public | BindingFlags.Instance));
        _members.AddRange(typeof(TOwner).GetFields(BindingFlags.Public | BindingFlags.Instance));
    }

    public PrintingConfig(TOwner obj)
    {
        _members.AddRange(obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance));
        _members.AddRange(obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance));
    }

    public IReadOnlyDictionary<string, Func<object, string>> AlternativePropertiesPrints =>
        _alternativePropertiesPrints;

    public IReadOnlyDictionary<Type, Func<object, string>> AlternativeTypesPrints => _alternativeTypesPrints;
    public IReadOnlyList<MemberInfo> Members => _members;

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
        _members.RemoveAll(property => property.Name == name);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        var type = typeof(TPropType);
        _members.RemoveAll(member => MemberInfoHelper.GetTypeFromMemberInfo(member) == type);
        return this;
    }
}