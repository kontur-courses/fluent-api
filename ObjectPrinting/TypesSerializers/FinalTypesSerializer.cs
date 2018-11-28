using System;
using System.Collections.Immutable;
using System.Linq;

namespace ObjectPrinting.TypesSerializers
{
    public class FinalTypesSerializer : TypeSerializer
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(short), typeof(byte),
            typeof(long), typeof(decimal), typeof(char), typeof(bool),
            typeof(Guid)
        };

        public override string Serialize(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
        {
            if (finalTypes.Contains(obj.GetType()))
            {
                return obj + Environment.NewLine;
            }

            return Successor?.Serialize(obj, nestingLevel, excludedValues);
        }
    }
}