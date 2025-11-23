using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;

namespace ObjectPrinting.HomeWork.PrintUtils.Interfaces;

public interface IPropertyRenderer
{
    public string? RenderProperty(
        object target,
        PropertyInfo prop,
        int nestingLevel,
        HashSet<object> visited,
        IRuleProcessor ruleProcessor,
        Func<object?, int, HashSet<object>, string> recursivePrinter);
}