using System;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Helpers;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies;

internal class PrimitiveStrategy : IStrategy
{
    public bool CanHandle(ValueContext context)
    {
        var type = context.Type;
        return type.IsTypePrimitive();
    }

    public string Print(ValueContext context, Func<ValueContext, string> recurse)
    {
        return context.Value?.ToString() ?? "null";
    }
}