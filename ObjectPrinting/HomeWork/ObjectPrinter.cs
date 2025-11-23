using ObjectPrinting.HomeWork.PrintUtils;
using ObjectPrinting.HomeWork.PrintUtils.Implementations;
using ObjectPrinting.HomeWork.PrintUtils.Strategies.Implementations;
using ObjectPrinting.HomeWork.PrintUtils.Strategies.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Implementations;

namespace ObjectPrinting.HomeWork;

public class ObjectPrinter
{
    public static PrintingConfig<T> For<T>()
    {
        var ruleProcessor = new RuleProcessor();
        var renderProperty = new PropertyRenderer();
        var strategies = new List<IPrintStrategy>
        {
            new EnumerablePrinterStrategy(),
            new SimplePrinterStrategy(ruleProcessor),
            new ObjectPrinterStrategy(renderProperty, ruleProcessor),
            new CycleFormatterStrategy()
        };
        return new PrintingConfig<T>(ruleProcessor, new PrintingProcessor(strategies));
    }
}