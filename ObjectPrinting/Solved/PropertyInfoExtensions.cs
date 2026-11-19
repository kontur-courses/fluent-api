using System;
using System.Reflection;
using Newtonsoft.Json;

namespace ObjectPrinting.Solved;

public static class PropertyInfoExtensions
{
    public static string FormatLine(
        this PropertyInfo propertyInfo,
        string propertyValue,
        int nestingLevel,
        string? endOfLine =  null)
    {
        var indentation = new string('\t', nestingLevel + 1);

        return indentation + propertyInfo.Name + " = " + propertyValue + (endOfLine ?? string.Empty);
    }
}