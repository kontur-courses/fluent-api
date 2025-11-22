using System;
using System.Collections;
using System.Text;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies
{
    internal class EnumerableStrategy : IStrategy
    {
        public bool CanHandle(ValueContext context)
        {
            return context.Value switch
            {
                null or string => false,
                _ => context.Value is IEnumerable
            };
        }

        public string Print(ValueContext context, Func<ValueContext, string> recurse)
        {
            var sequence = (IEnumerable)context.Value!;
            var type = context.Type ?? sequence.GetType();
            var sb = new StringBuilder();
            sb.AppendLine($"{type.Name} [");

            var i = 0;
            foreach (var item in sequence)
            {
                var childContext = new ValueContext
                {
                    Value = item,
                    Type = item?.GetType(),
                    Path = $"{context.Path}[{i}]",
                    Indent = context.Indent + 1,
                    Settings = context.Settings,
                    Visited = context.Visited
                };

                var printed = recurse(childContext);
                var prefix = new string('\t', context.Indent + 1);

                sb.Append(prefix).Append($"[{i}] = ");

                sb.AppendLine(printed == "" ? "null" : printed);

                i++;
            }

            sb.Append(new string('\t', context.Indent)).Append("]");

            return sb.ToString();
        }
    }
}