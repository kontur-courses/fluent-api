using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Dto;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.RuleUtils.Strategies.Implementations;

public class ExcludeRule : ISerializationRule
{
    private readonly Type? typeToExclude;
    private readonly PropertyInfo? propertyToExclude;

    public ExcludeRule(Type type)
    {
        typeToExclude = type;
    }

    public ExcludeRule(PropertyInfo property)
    {
        propertyToExclude = property;
    }

    public bool CanApply(PropertyInfo? propertyInfo)
    {
        if (propertyToExclude != null)
            return propertyInfo == propertyToExclude;

        if (typeToExclude != null)
            return propertyInfo != null && propertyInfo.PropertyType == typeToExclude;

        return false;
    }

    public RuleOutcome Apply(object value)
    {
        return new RuleOutcome(RuleResult.Skip, null);
    }
}