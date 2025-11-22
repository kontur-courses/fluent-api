using System;
using System.Collections.Generic;
using ObjectPrinting.PrintingHandlers.ApplyingSettings;
using ObjectPrinting.PrintingHandlers.HandlingStrategies;

namespace ObjectPrinting.PrintingHandlers
{
    internal class PrintingEngine(PrintingSettings settings)
    {
        private readonly PrintingSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));
        private readonly ApplierRunner applierRunner = new();
        private readonly StrategySelector strategySelector = new();

        public string Print(object? root)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var context = new ValueContext
            {
                Value = root,
                Type = root?.GetType(),
                Path = root?.GetType()?.Name ?? "null",
                Indent = 0,
                Settings = settings,
                Visited = visited
            };

            return PrintInternal(context).TrimEnd('\r', '\n');
        }

        private string PrintInternal(ValueContext context)
        {
            var result = applierRunner.Run(context);

            if (result.Applied)
            {
                if (result.Exclude)
                    return "";

                if (result.Serialized != null)
                    return result.Serialized;

                if (result.NewValue != null)
                {
                    context.Value = result.NewValue;
                    context.Type = result.NewValue?.GetType();
                }
            }

            var strategy = strategySelector.Select(context);
            return strategy.Print(context, PrintInternal);
        }
    }
}