using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Actions;

public class SpecifyCultureAction : IAction
{
    private readonly Type? typeToFormat = null;
    private readonly PropertyInfo? propertyToFormat = null;
    private readonly CultureInfo cultureInfo;

    public SpecifyCultureAction(Type typeToFormat, CultureInfo cultureInfo)
    {
        this.typeToFormat = typeToFormat;
        this.cultureInfo = cultureInfo;
    }

    public SpecifyCultureAction(PropertyInfo propertyToFormat, CultureInfo cultureInfo)
    {
        this.propertyToFormat = propertyToFormat;
        this.cultureInfo = cultureInfo;
    }


    public bool CanHandle(PropertyInfo property)
    {
        return propertyToFormat != null && propertyToFormat == property &&
               typeof(IFormattable).IsAssignableFrom(property.PropertyType) ||
               typeToFormat != null && typeToFormat.IsAssignableFrom(property.PropertyType);
    }

    public ActionResult Handle(object value)
    {
        return new ActionResult
        {
            IsSkipped = false,
            Value = ((IFormattable)value).ToString(null, cultureInfo)
        };
    }
}