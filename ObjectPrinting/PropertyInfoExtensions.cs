using System.Reflection;

namespace ObjectPrinting;

public static class PropertyInfoExtensions
{
    public static string FormatLine(
        this PropertyInfo propertyInfo,
        string propertyValue,
        int nestingLevel)
    {
        var indentation = new string('\t', nestingLevel + 1);

        return indentation + propertyInfo.Name + " = " + propertyValue;
    }
}