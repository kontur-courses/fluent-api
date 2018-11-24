using System;
using System.Collections.Immutable;
using System.Linq;

namespace ObjectPrinting.TypesHandlers
{
    public class FinalTypesHandler : TypeHandler
    {
        public override string Handle(
            object obj, 
            int nestingLevel, 
            ImmutableHashSet<object> excludedValues, 
            TypeHandler handler)
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()))
            {
                return obj + Environment.NewLine;
            }

            return Successor?.Handle(obj, nestingLevel, excludedValues, handler);
        }
    }
}