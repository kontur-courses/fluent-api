using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class Serialization
    {
        private readonly SerializationConfig serializationConfig;
        public Serialization(SerializationConfig serializationConfig)
        {
            this.serializationConfig = serializationConfig;
        }

        public string Serialize(object obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > 100)
                return "";

            if (obj == null)
                return "null";
            
            if (serializationConfig.TypePrintingRules.TryGetValue(obj.GetType(), out var typePrintingRule))
                return typePrintingRule(obj).ToString();

            if (serializationConfig.FinalTypes.Contains(obj.GetType()))
            {
                if (serializationConfig.TypeCultures.TryGetValue(obj.GetType(), out var cultureInfo))
                    return ((dynamic) obj).ToString(cultureInfo);
                return obj.ToString();
            }

            return obj switch
            {
                IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel),
                IEnumerable enumerable => SerializeIEnumerable(enumerable, nestingLevel),
                _ => SerializeMembers(obj, nestingLevel)
            };
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            sb.AppendLine("{");
            foreach (var key in dictionary.Keys)
            {
                sb.Append(indentation + "{" + PrintToString(key, nestingLevel + 1) + Environment.NewLine
                          + indentation + ":" + PrintToString(dictionary[key], nestingLevel + 1) + "},"
                          + Environment.NewLine);
            }

            sb.Append(indentation + "}");
            return sb.ToString();
        }

        private string SerializeIEnumerable(IEnumerable obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("[");
            foreach (var key in obj)
                stringBuilder.AppendLine(indentation + PrintToString(key, nestingLevel + 1) + ",");

            stringBuilder.Append(indentation + "]");
            var serializedIEnumerable = stringBuilder.ToString();

            var lastIndexOf = serializedIEnumerable.LastIndexOf(",", StringComparison.Ordinal);
            if (lastIndexOf != -1)
                return serializedIEnumerable.Remove(lastIndexOf, 1);
            return serializedIEnumerable;
        }

        private bool IsExcludedMember(MemberInfo memberInfo)
        {
            if ((memberInfo.MemberType & MemberTypes.Field) != 0)
                return serializationConfig.ExcludedTypes.Contains((memberInfo as FieldInfo)?.FieldType) ||
                       serializationConfig.ExcludedMembers.Contains(memberInfo);

            if ((memberInfo.MemberType & MemberTypes.Property) != 0)
                return serializationConfig.ExcludedTypes.Contains((memberInfo as PropertyInfo)?.PropertyType) ||
                       serializationConfig.ExcludedMembers.Contains(memberInfo);

            return false;
        }
        
        private string SerializeMembers(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            foreach (var memberInfo in type.GetMembers()
                .Where(IsFieldOrProperty).Where(x => !IsExcludedMember(x)))
            {
                var value = GetValue(obj, memberInfo);

                if (value != null && value.Equals(obj))
                    continue;

                sb.Append(Environment.NewLine + indentation + memberInfo.Name + " = " +
                          SerializeValue(nestingLevel, memberInfo, value));
            }

            return sb.ToString();
        }

        private string SerializeValue(int nestingLevel, MemberInfo memberInfo, object value)
        {
            var sb = "";

            if (serializationConfig.MemberPrintingRules.TryGetValue(memberInfo, out var rule))
                sb = rule(value);
            else if (serializationConfig.MemberCultures.TryGetValue(memberInfo, out var cultureInfo))
                sb = ((dynamic) value).ToString(cultureInfo);
            else
                sb = (PrintToString(value, nestingLevel + 1));

            sb = serializationConfig.GlobalTrimToLength(sb);

            if (serializationConfig.MemberTrimToLength.TryGetValue(memberInfo, out var trimRule))
                return trimRule(sb);
            return sb;
        }

        private static bool IsFieldOrProperty(MemberInfo memberInfo) =>
            memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property;
        
        private static object GetValue(object obj, MemberInfo memberInfo)
        {
            if ((memberInfo.MemberType & MemberTypes.Field) != 0)
                return (memberInfo as FieldInfo)?.GetValue(obj);

            if ((memberInfo.MemberType & MemberTypes.Property) != 0)
                return (memberInfo as PropertyInfo)?.GetValue(obj);

            return null;
        }
    }
}