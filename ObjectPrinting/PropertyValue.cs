using System.Reflection;

namespace ObjectPrinting;

internal record PropertyValue(string? Name, Type? DeclaringType, object? Value)
{
    public PropertyValue(PropertyInfo? propertyInfo, object? value)
        : this(propertyInfo?.Name, propertyInfo?.DeclaringType, value)
    {
    }
}
