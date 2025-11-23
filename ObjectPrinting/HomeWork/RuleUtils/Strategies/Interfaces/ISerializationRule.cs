using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Dto;

namespace ObjectPrinting.HomeWork.RuleUtils.Strategies.Interfaces;

public interface ISerializationRule
{
    bool CanApply(PropertyInfo? propertyInfo);
    RuleOutcome Apply(object value);
}