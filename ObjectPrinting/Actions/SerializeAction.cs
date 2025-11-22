using System;
using System.Reflection;

namespace ObjectPrinting.Actions;

public class SerializeAction : IAction
{
    private readonly Func<object, string> serializer;

    private readonly Type? typeToSerialize = null;
    private readonly PropertyInfo? propertyToSerialize = null;

    public SerializeAction(Type typeToSerialize, Func<object, string> serializer)
    {
        this.typeToSerialize = typeToSerialize;
        this.serializer = serializer;
    }

    public SerializeAction(PropertyInfo propertyToSerialize, Func<object, string> serializer)
    {
        this.propertyToSerialize = propertyToSerialize;
        this.serializer = serializer;
    }

    public bool CanHandle(PropertyInfo property)
    {
        return propertyToSerialize != null && propertyToSerialize == property ||
               typeToSerialize != null && typeToSerialize.IsAssignableFrom(property.PropertyType);
    }

    public ActionResult Handle(object value)
    {
        return new ActionResult
        {
            IsSkipped = false,
            Value = serializer(value)
        };
    }
}