using System;
using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace ObjectPrinting.TypesSerializers
{
    public class EnumerableSerializer : TypeSerializer
    {
        private int maxCount;
        private readonly Lazy<TypeSerializer> typeSerializer;

        public EnumerableSerializer(int maxElementsCountForEnumerables, TypeSerializer typeSerializer)
        {
            maxCount = maxElementsCountForEnumerables;
            this.typeSerializer = new Lazy<TypeSerializer>(() => typeSerializer);
        }

        public override string Serialize(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
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
                    if (counter >= maxCount)
                    {
                        sb.Append($"{identation}...Reached maximum count of elements...");

                        break;
                    }

                    sb.Append(
                        $"{identation}Element {counter++} = {typeSerializer.Value.Serialize(element, nestingLevel + 1, excludedValues)}");
                }

                return sb.ToString();
            }

            return Successor.Serialize(obj, nestingLevel, excludedValues);
        }
    }
}