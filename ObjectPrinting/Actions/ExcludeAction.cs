using System;
using System.Reflection;

namespace ObjectPrinting.Actions;

public class ExcludeAction : IAction
{
    private readonly Type? typeToExclude = null;
    private readonly PropertyInfo? propertyToExclude = null;

    public ExcludeAction(Type typeToExclude)
    {
        this.typeToExclude = typeToExclude;
    }

    public ExcludeAction(PropertyInfo propertyToExclude)
    {
        this.propertyToExclude = propertyToExclude;
    }

    public bool CanHandle(PropertyInfo property)
    {
        return propertyToExclude != null && propertyToExclude == property ||
               typeToExclude != null && typeToExclude.IsAssignableFrom(property.PropertyType);
    }

    public ActionResult Handle(object value)
    {
        return new ActionResult
        {
            IsSkipped = true
        };
    }
}