using System;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces
{
    internal interface IStrategy
    {
        bool CanHandle(ValueContext context);
        string Print(ValueContext context, Func<ValueContext, string> recurse);
    }
}