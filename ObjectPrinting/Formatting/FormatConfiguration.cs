using System.Reflection;

namespace ObjectPrinting.Formatting
{
    internal interface FormatConfiguration
    {
        int MaximumRecursionDepth { get; }
        string GetPropertyPrintingStart(string indent, PropertyInfo property); 
        string GetIndent(int level);
    }
}