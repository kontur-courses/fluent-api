using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class SerializerHandler
    {
        public static string SerializeIEnumerable(IEnumerable collection, int nestingLevel,
            Func<object, int, string> printToString)
        {
            var sb = new StringBuilder(collection.GetType().Name + " {" + Environment.NewLine);
            foreach (var item in collection)
            {
                sb.Append(new string('\t', nestingLevel + 1));
                sb.Append(printToString(item, nestingLevel + 1));
            }

            sb.Append(new string('\t', nestingLevel) + '}' + Environment.NewLine);
            return sb.ToString();
        }

        public static string SerializeDictionary(IDictionary dictionary, int nestingLevel,
            Func<object, int, string> printToString)
        {
            var sb = new StringBuilder(dictionary.GetType().Name + " {" + Environment.NewLine);
            foreach (var key in dictionary.Keys)
            {
                sb.Append(new string('\t', nestingLevel + 1));
                sb.Append(printToString(key, nestingLevel + 1).Trim() + " = ");
                sb.Append(printToString(dictionary[key], nestingLevel + 1));
            }

            sb.Append(new string('\t', nestingLevel) + '}' + Environment.NewLine);

            return sb.ToString();
        }

        public static string SerializeObject(object obj, int nestingLevel,
            Func<object, MemberInfo, int, string> serializeMember, Func<MemberInfo, bool> isMemberExcluded)
        {
            var indent = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                         .Where(t => t.MemberType == MemberTypes.Field || t.MemberType == MemberTypes.Property)
                         .Where(x => !isMemberExcluded(x)))
            {
                sb.Append(
                    indent + memberInfo.Name + " = " +
                    serializeMember(obj, memberInfo, nestingLevel + 1)
                );
            }

            return sb.ToString();
        }
    }
}
