using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace ObjectPrinting.TypesSerializers
{
    public class EnumerableSerializer : TypeSerializer
    {
        public override string Serialize(
            object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues,
            TypeSerializer serializer)
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
                        $"{identation}Element {counter++} = {serializer.Serialize(element, nestingLevel + 1, excludedValues, serializer)}");
                }

                return sb.ToString();
            }

            return Successor.Serialize(obj, nestingLevel, excludedValues, serializer);
        }
    }
}