namespace ObjectPrinting.HomeWork.PrintUtils.Interfaces;

public interface IPrintingProcessor
{
    string Print(object? obj, int nestingLevel, HashSet<object> visited);
}