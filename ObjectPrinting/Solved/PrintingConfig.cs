#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Solved;

public class PrintingConfig<TOwner>
{
    private readonly Dictionary<string, Func<object, string>> _alternativePropertiesPrints = new();
    private readonly Dictionary<Type, Func<object, string>> _alternativeTypesPrints = new();
    private readonly List<MemberInfo> _members = new();

    public PrintingConfig()
    {
        _members.AddRange(typeof(TOwner).GetProperties());
        _members.AddRange(typeof(TOwner).GetFields());
    }

    public IReadOnlyDictionary<string, Func<object, string>> AlternativePropertiesPrints =>
        _alternativePropertiesPrints;

    public IReadOnlyDictionary<Type, Func<object, string>> AlternativeTypesPrints => _alternativeTypesPrints;
    public IReadOnlyList<MemberInfo> Members => _members;

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var name = GetMemberNameFromExpression(memberSelector.Body);
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
        var name = GetMemberNameFromExpression(memberSelector.Body);
        _members.RemoveAll(property => property.Name == name);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        var type = typeof(TPropType);
        _members.RemoveAll(member => GetTypeFromMemberInfo(member) == type);
        return this;
    }

    private string GetMemberNameFromExpression(Expression expression)
    {
        if (expression is MemberExpression memberSelector)
            return memberSelector.Member.Name;
        throw new ArgumentException($"Can't convert {expression} to MemberExpression.");
    }

    private Type GetTypeFromMemberInfo(MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => throw new ArgumentException("Member is not a field or a property!")
        };
    }
}