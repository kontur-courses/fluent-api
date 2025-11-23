using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Dto;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.RuleUtils.Implementations;

public class RuleProcessor : IRuleProcessor
{
    private readonly List<ISerializationRule> rules = [];

    public void AddRule(ISerializationRule rule)
    {
        rules.Add(rule);
    }

    public RuleOutcome ApplyRule(object? propertyValue, PropertyInfo? propertyInfo)
    {
        if (propertyValue == null)
        {
            return new RuleOutcome(RuleResult.Print, "null");
        }

        var current = propertyValue; 
        string? resultString = null;

        foreach (var rule in rules.Where(r => r.CanApply(propertyInfo)))
        {
            var outcome = rule.Apply(current);

            if (outcome.Action == RuleResult.Skip)
                return new RuleOutcome(RuleResult.Skip, null);

            current = outcome.Value ?? current;
            resultString = outcome.Value;
        }


        return new RuleOutcome(
            RuleResult.Print,
            resultString ?? propertyValue.ToString()
        );
    }
}