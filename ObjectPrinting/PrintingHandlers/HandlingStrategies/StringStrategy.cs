using System;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies;

internal class StringStrategy : IStrategy
{
    public bool CanHandle(ValueContext context) => context.Value is string;

    public string Print(ValueContext context, Func<ValueContext, string> recurse)
    {
        return (string?)context.Value ?? "null";
    }
}