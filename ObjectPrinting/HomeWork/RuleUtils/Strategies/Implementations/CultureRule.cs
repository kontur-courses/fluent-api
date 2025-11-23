using System.Globalization;
using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Dto;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.RuleUtils.Strategies.Implementations;

public class CultureRule(CultureInfo cultureInfo, Type? targetType = null) : ISerializationRule
{
    public bool CanApply(PropertyInfo? propertyInfo)
    {
        if (propertyInfo == null)
            return false;

        if (!typeof(IFormattable).IsAssignableFrom(propertyInfo.PropertyType))
            return false;

        if (targetType == null)
            return true;

        return propertyInfo.PropertyType == targetType;
    }

    public RuleOutcome Apply(object value)
    {
        return new RuleOutcome(RuleResult.Print, ((IFormattable)value).ToString(null, cultureInfo));
    }
}