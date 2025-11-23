using System.Text;
using ObjectPrinting.HomeWork.PrintUtils.Strategies.Interfaces;

namespace ObjectPrinting.HomeWork.PrintUtils.Strategies.Implementations;

public class CycleFormatterStrategy : IPrintStrategy
{
    private readonly Dictionary<object, int> visited = new();
    public bool CanHandle(Type type) => !type.IsValueType && type != typeof(string);


    public string Print(object obj, int nestingLevel, HashSet<object> ignoredVisited,
        Func<object?, int, HashSet<object>, string> ignoredRecursivePrinter, StringBuilder sb)
    {
        if (visited.TryGetValue(obj, out var originalLevel))
        {
            return FormatReference(obj, originalLevel);
        }

        visited[obj] = nestingLevel;

        return null;
    }

    private string FormatReference(object obj, int nestingLevel)
    {
        var type = obj.GetType();
        return $"<see {type.Name} level={nestingLevel}>";
    }
}