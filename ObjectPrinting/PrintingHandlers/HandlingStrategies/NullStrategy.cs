using System;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies;

internal class NullStrategy : IStrategy
{
    public bool CanHandle(ValueContext context) => context.Value == null;

    public string Print(ValueContext context, Func<ValueContext, string> recurse) => "null";
}