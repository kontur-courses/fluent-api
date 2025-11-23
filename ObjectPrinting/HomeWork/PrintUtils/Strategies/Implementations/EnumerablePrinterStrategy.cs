using System.Collections;
using System.Text;
using ObjectPrinting.HomeWork.PrintUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.PrintUtils.Strategies.Implementations;

public class EnumerablePrinterStrategy : IPrintStrategy
{
    public bool CanHandle(Type type) => typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);

    public string Print(object obj, int nestingLevel, HashSet<object> visited,
        Func<object?, int, HashSet<object>, string> recursivePrinter, StringBuilder sb)
    {
        return PrintEnumerable((IEnumerable)obj, nestingLevel, visited, recursivePrinter, sb);
    }

    private string PrintEnumerable(IEnumerable enumerable, int nestingLevel, HashSet<object> visited,
        Func<object?, int, HashSet<object>, string> recursivePrinter, StringBuilder sb)
    {
        var indent = new string('\t', nestingLevel);

        sb.AppendLine(indent + enumerable.GetType().Name + " [");

        foreach (var item in enumerable)
        {
            var itemText = recursivePrinter(item, nestingLevel, visited);

            var itemLines = itemText.Split(Environment.NewLine);
            foreach (var line in itemLines)
            {
                sb.AppendLine(new string('\t', nestingLevel + 1) + line);
            }
        }

        sb.AppendLine(indent + "]");
        return sb.ToString();
    }
}