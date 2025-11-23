using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Dto;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.RuleUtils.Strategies.Implementations;

public class TrimStringRule : ISerializationRule
{
    private readonly PropertyInfo? property;
    private readonly Type? type;
    private readonly int length;

    public TrimStringRule(PropertyInfo property, int length)
    {
        this.property = property;
        this.length = length;
    }

    public TrimStringRule(Type type, int length)
    {
        this.type = type;
        this.length = length;
    }

    public bool CanApply(PropertyInfo? propertyInfo)
    {
        if (property != null)
            return propertyInfo == property;

        if (type != null)
            return propertyInfo?.PropertyType == type;

        return false;
    }

    public RuleOutcome Apply(object value)
    {
        var s = (string)value;
        var trimmed = s[..Math.Min(length, s.Length)];
        return new RuleOutcome(RuleResult.Print, trimmed);
    }
}