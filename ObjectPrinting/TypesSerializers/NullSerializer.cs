using System;
using System.Collections.Immutable;

namespace ObjectPrinting.TypesSerializers
{
    public class NullSerializer : TypeSerializer
    {
        public override string Serialize(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
        {
            if (obj == null)
            {
                return "null" + Environment.NewLine;
            }

            return Successor?.Serialize(obj, nestingLevel, excludedValues);
        }
    }
}