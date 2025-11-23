using System.Collections;
using System.Reflection;
using ObjectPrinting.HomeWork.PrintUtils.Helpers;
using ObjectPrinting.HomeWork.PrintUtils.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Dto;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;

namespace ObjectPrinting.HomeWork.PrintUtils.Implementations;

public class PropertyRenderer : IPropertyRenderer
{
    public string? RenderProperty(
        object target,
        PropertyInfo prop,
        int nestingLevel,
        HashSet<object> visited,
        IRuleProcessor ruleProcessor, 
        Func<object?, int, HashSet<object>, string> recursivePrinter)
    {
        var value = prop.GetValue(target);
        if (value == null) return null;

        var ruleOutcome = ruleProcessor.ApplyRule(value, prop);
        if (ruleOutcome.Action == RuleResult.Skip) return null;

        var type = value.GetType();

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string) || !SimpleTypeHelper.IsSimple(type))
        {
            var printed = recursivePrinter(value, nestingLevel + 1, visited);
            return $"{prop.Name} =\n{printed}";
        }

        var formatted = ruleOutcome.Value ?? value.ToString();
        return $"{prop.Name} = {formatted}";
    }
}