using System.Linq;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies;

internal class StrategySelector
{
    private readonly IStrategy[] strategies =
    [
        new NullStrategy(),
        new PrimitiveStrategy(),
        new StringStrategy(),
        new EnumerableStrategy(),
        new ObjectStrategy()
    ];

    public IStrategy Select(ValueContext context)
    {
        return strategies.First(s => s.CanHandle(context));
    }
}