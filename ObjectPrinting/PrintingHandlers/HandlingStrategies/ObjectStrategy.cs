using System;
using System.Reflection;
using System.Text;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Helpers;
using ObjectPrinting.PrintingHandlers.HandlingStrategies.Interfaces;

namespace ObjectPrinting.PrintingHandlers.HandlingStrategies
{
    internal class ObjectStrategy : IStrategy
    {
        public bool CanHandle(ValueContext context)
        {
            if (context.Value is null or string) return false;

            var type = context.Type;
            return !type.IsTypePrimitive();
        }

        public string Print(ValueContext context, Func<ValueContext, string> recurse)
        {
            var obj = context.Value!;
            var type = context.Type ?? obj.GetType();

            if (!type.IsValueType)
            {
                if (!context.Visited.Add(obj))
                    return "[Cyclic Reference]";
            }

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in properties)
            {
                var childPath = $"{type.Name}.{p.Name}";
                var value = p.GetValue(obj);

                var childContext = new ValueContext
                {
                    Value = value,
                    Type = value?.GetType(),
                    Path = childPath,
                    Indent = context.Indent,
                    Settings = context.Settings,
                    Visited = context.Visited
                };

                var printed = recurse(childContext);
                if (printed == "") continue;

                var prefix = new string('\t', context.Indent + 1);
                if (printed.Contains(Environment.NewLine))
                {
                    sb.Append(prefix).Append(p.Name).Append(" = ").AppendLine();
                    var childIndent = new string('\t', context.Indent + 2);
                    var lines = printed.Split([Environment.NewLine], StringSplitOptions.None);

                    foreach (var line in lines)
                        sb.Append(childIndent).AppendLine(line);
                }
                else
                {
                    sb.Append(prefix).Append(p.Name).Append(" = ").AppendLine(printed);
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
    }
}