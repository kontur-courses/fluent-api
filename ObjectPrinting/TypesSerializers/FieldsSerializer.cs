using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.TypesSerializers
{
    public class FieldsSerializer : TypeSerializer
    {
        private readonly Lazy<TypeSerializer> typeSerializer;

        public FieldsSerializer(TypeSerializer typeSerializer)
        {
            this.typeSerializer = new Lazy<TypeSerializer>(() => typeSerializer);
        }

        public override string Serialize(object obj,
            int nestingLevel,
            ImmutableHashSet<object> excludedValues)
        {
            var sb = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();

            foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member => member.MemberType == MemberTypes.Field))
            {
                var fieldInfo = type.GetField(memberInfo.Name);
                var fieldValue = fieldInfo.GetValue(obj);

                if (excludedValues.Contains(fieldValue))
                {
                    sb.Append(identation + fieldInfo.Name + " = " + Constants.Circular);

                    continue;
                }

                sb.Append(identation + fieldInfo.Name + " = " +
                    typeSerializer.Value.Serialize(fieldValue,
                        nestingLevel + 1, excludedValues.Add(fieldValue)));
            }

            return sb
                .Append(Successor?.Serialize(obj, nestingLevel, excludedValues))
                .ToString();
        }
    }
}