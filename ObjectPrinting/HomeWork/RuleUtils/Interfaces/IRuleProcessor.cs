using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Dto;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.RuleUtils.Interfaces;

public interface IRuleProcessor
{
    public RuleOutcome ApplyRule(object propertyValue, PropertyInfo? propertyInfo);
    public void AddRule(ISerializationRule rule);
}