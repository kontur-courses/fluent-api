using System;
using System.Reflection;

namespace ObjectPrinting.Actions;

public class TrimAction : IAction
{
    private readonly int maxLength;
    private readonly PropertyInfo propertyToTrim;

    public TrimAction(PropertyInfo propertyToTrim, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        this.propertyToTrim = propertyToTrim;
        this.maxLength = maxLength;
    }

    public bool CanHandle(PropertyInfo property)
    {
        return propertyToTrim == property && property.PropertyType == typeof(string);
    }

    public ActionResult Handle(object value)
    {
        return new ActionResult
        {
            IsSkipped = false,
            Value = ((string)value)[..maxLength]
        };
    }
}