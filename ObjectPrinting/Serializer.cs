using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ObjectPrinting
{
    public static class Serializer<T>
    {
        private static readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public static string Serialize(PrintingConfig<T> config, T obj)
        {
            return PrintToString(obj, config, 0, new Stack<object>());
        }

        private static string PrintToString(
            object obj,
            PrintingConfig<T> config,
            int nestingLevel,
            Stack<object> serializedObjects)
        {
            if (obj == null)
                return $"null{Environment.NewLine}";
            if (finalTypes.Contains(obj.GetType()))
                return $"{obj}{Environment.NewLine}";
            if (serializedObjects.Contains(obj))
                throw new SerializationException("Circular reference");
            serializedObjects.Push(obj);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            if (obj is IEnumerable enumerable)
                sb.Append(SerializeCollection(enumerable, config, nestingLevel, serializedObjects));
            else
                sb.Append(SerializeObject(obj, config, nestingLevel, serializedObjects));
            serializedObjects.Pop();
            return sb.ToString();
        }

        private static string SerializeCollection(
            object collection,
            PrintingConfig<T> config,
            int nestingLevel,
            Stack<object> serializedObjects)
        {
            var sb = new StringBuilder();
            var serializedElements = from object e in (IEnumerable) collection
                select GetIndentation(nestingLevel + 1)
                       + PrintToString(e, config, nestingLevel + 1, serializedObjects);
            sb.AppendLine(GetIndentation(nestingLevel) + "{");
            sb.AppendJoin(Environment.NewLine, serializedElements);
            sb.AppendLine(GetIndentation(nestingLevel) + "}");
            return sb.ToString();
        }

        private static string SerializeObject(
            object obj,
            PrintingConfig<T> config,
            int nestingLevel,
            Stack<object> serializedObjects)
        {
            var sb = new StringBuilder();
            foreach (var memberInfo in obj.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.IsPropertyOrField())
                .Where(config.IsMemberNotExcluded))
            {
                var memberValue = memberInfo.GetValue(obj);
                var toPrint = TrySerializeMember(config, memberInfo, memberValue, out var serializedMember)
                    ? serializedMember
                    : memberValue;
                if (config.TryGetMemberLength(memberInfo, out var maxLength))
                    toPrint = toPrint.ToString()?.Substring(0, maxLength);
                var serializedObject = PrintToString(toPrint, config, nestingLevel + 1, serializedObjects);
                sb.Append($"{GetIndentation(nestingLevel + 1)}{memberInfo.Name} = {serializedObject}");
            }

            return sb.ToString();
        }

        private static bool TrySerializeMember(
            PrintingConfig<T> config,
            MemberInfo memberInfo,
            object value,
            out string serializedMember)
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