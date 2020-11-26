using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ObjectPrinting
{
    public class Serializer<T>
    {
        private readonly PrintingConfig<T> config;
        private readonly Stack<object> serializedObjects = new Stack<object>();

        public Serializer(PrintingConfig<T> config)
        {
            this.config = config;
        }

        public string Serialize(T obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";
            if (config.FinalTypes.Contains(obj.GetType()))
                return $"{obj}{Environment.NewLine}";
            if (serializedObjects.Contains(obj))
                throw new SerializationException("Circular reference");
            serializedObjects.Push(obj);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            if (obj is IEnumerable enumerable)
                sb.Append(SerializeCollection(enumerable, nestingLevel));
            else
                sb.Append(SerializeObject(obj, nestingLevel));
            serializedObjects.Pop();
            return sb.ToString();
        }

        private string SerializeCollection(object collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetIndentation(nestingLevel) + "{");
            var serializedElements = ((IEnumerable) collection)
                .Cast<object>()
                .Select(x => GetIndentation(nestingLevel + 1)
                             + PrintToString(x, nestingLevel + 1));
            foreach (var serializedElement in serializedElements) sb.Append(serializedElement);
            sb.AppendLine(GetIndentation(nestingLevel) + "}");
            return sb.ToString();
        }

        private string SerializeObject(
            object obj,
            int nestingLevel)
        {
            var sb = new StringBuilder();
            foreach (var memberInfo in obj.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.IsPropertyOrField())
                .Where(config.IsMemberNotExcluded))
            {
                var memberValue = memberInfo.GetValue(obj);
                var toPrint = TrySerializeMember(memberInfo, memberValue, out var serializedMember)
                    ? serializedMember
                    : memberValue;
                if (config.TryGetMemberLength(memberInfo, out var maxLength))
                    toPrint = toPrint?.ToString().Substring(0, maxLength);
                var serializedObject = PrintToString(toPrint, nestingLevel + 1);
                sb.Append($"{GetIndentation(nestingLevel + 1)}{memberInfo.Name} = {serializedObject}");
            }

            return sb.ToString();
        }

        private bool TrySerializeMember(MemberInfo memberInfo, object value, out string serializedMember)
        {
            if (config.TryGetSerializer(memberInfo, out var serializer))
            {
                serializedMember = serializer(value);
                return true;
            }

            serializedMember = null;
            return false;
        }

        private static string GetIndentation(int nestingLevel)
        {
            return new string('\t', nestingLevel);
        }
    }
}