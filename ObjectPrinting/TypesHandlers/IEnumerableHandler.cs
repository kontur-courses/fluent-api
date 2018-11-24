using System;
using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace ObjectPrinting.TypesHandlers
{
    public class IEnumerableHandler : TypeHandler
    {
        public override string Handle(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues,
            TypeHandler handler)
        {
            if (obj is IEnumerable enumerable)
            {
                var identation = new string('\t', nestingLevel + 1);
                var sb = new StringBuilder();
                sb.AppendLine(obj.GetType()
                    .Name);
                var counter = 0;

                foreach (var element in enumerable)
                {
                    sb.Append(
                        $"{identation}Element {counter++} = {handler.Handle(element, nestingLevel + 1, excludedValues, handler)}");
                }

                return sb.ToString();
            }

            return Successor.Handle(obj, nestingLevel, excludedValues, handler);
        }
    }
}