using System.Text;

namespace ObjectPrinting.HomeWork.PrintUtils.Strategies.Interfaces;

public interface IPrintStrategy
{
    bool CanHandle(Type type);
    string Print(object obj, int nestingLevel, HashSet<object> visited, Func<object?, int, HashSet<object>, string> recursivePrinter, StringBuilder sb);
}