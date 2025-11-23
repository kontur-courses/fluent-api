using System.Collections;
using System.Reflection;
using System.Text;
using ObjectPrinting.HomeWork.PrintUtils.Helpers;
using ObjectPrinting.HomeWork.PrintUtils.Interfaces;
using ObjectPrinting.HomeWork.PrintUtils.Strategies.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;

namespace ObjectPrinting.HomeWork.PrintUtils.Strategies.Implementations;

public class ObjectPrinterStrategy(IPropertyRenderer propertyRenderer, IRuleProcessor ruleProcessor) : IPrintStrategy
{
    public bool CanHandle(Type type) => !typeof(IEnumerable).IsAssignableFrom(type) && !SimpleTypeHelper.IsSimple(type);

    public string Print(object obj, int nestingLevel, HashSet<object> visited,
        Func<object?, int, HashSet<object>, string> recursivePrinter, StringBuilder sb)
    {
        return PrintObject(obj, nestingLevel, visited, recursivePrinter, ruleProcessor, propertyRenderer, sb);
    }

    private string PrintObject(
        object obj,
        int nestingLevel,
        HashSet<object> visited,
        Func<object?, int, HashSet<object>, string> recursivePrinter,
        IRuleProcessor ruleProcessor,
        IPropertyRenderer propertyRenderer,
        StringBuilder sb)
    {
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        var indent = new string('\t', nestingLevel + 1);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propLine = propertyRenderer.RenderProperty(
                obj, prop, nestingLevel, visited,
                ruleProcessor, recursivePrinter
            );

            if (!string.IsNullOrEmpty(propLine))
            {
                sb.AppendLine(indent + propLine);
            }
        }

        return sb.ToString().TrimEnd();
    }
}