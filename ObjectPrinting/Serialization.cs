using System;
using System.Collections;
using System.Collections.Generic;
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

        private const char Tab = '\t';

        public string Serialize(object obj)
        {
            return PrintToString(obj, 0);
        }

        private HashSet<object> objects = new HashSet<object>();

        private string PrintToString(object obj, int nestingLevel)
        {
            if (objects.Contains(obj))
                return $"{obj.GetType()} CircularReference!";

            objects.Add(obj);

            var serializedObject = SerializeObject(obj, nestingLevel);
            
            objects.Remove(obj);
            return serializedObject;
        }

        private string SerializeObject(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            if (serializationConfig.TypePrintingRules.TryGetValue(obj.GetType(), out var typePrintingRule))
                return typePrintingRule(obj);

            if (serializationConfig.FinalTypes.Contains(obj.GetType()))
            {
                if (serializationConfig.TypeTrimToLength.TryGetValue(obj.GetType(), out var typeTrimToLength) &&
                    nestingLevel == 0)
                    return typeTrimToLength(GetFinalTypeString(obj));
                return GetFinalTypeString(obj);
            }

            return obj switch
            {
                IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel),
                IEnumerable enumerable => SerializeIEnumerable(enumerable, nestingLevel),
                _ => SerializeMembers(obj, nestingLevel)
            };
        }

        private string GetFinalTypeString(object obj)
        {
            if (serializationConfig.TypeCultures.TryGetValue(obj.GetType(), out var cultureInfo))
                return ((dynamic)obj).ToString(cultureInfo);
            return obj.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = new string(Tab, nestingLevel + 1);
            sb.AppendLine("{");
            foreach (var key in dictionary.Keys)
            {
                sb.Append(indentation);
                sb.Append("{");
                sb.Append(PrintToString(key, nestingLevel + 1));
                sb.Append(Environment.NewLine);
                sb.Append(indentation);
                sb.Append(":");
                sb.Append(PrintToString(dictionary[key], nestingLevel + 1));
                sb.Append("},");
                sb.Append(Environment.NewLine);
            }

            sb.Append(indentation);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string SerializeIEnumerable(IEnumerable obj, int nestingLevel)
        {
            var indentation = new string(Tab, nestingLevel + 1);
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
            var indentation = new string(Tab, nestingLevel + 1);
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
            var serializedValue = "";

            if (serializationConfig.MemberPrintingRules.TryGetValue(memberInfo, out var rule))
                serializedValue = rule(value);
            else if (serializationConfig.MemberCultures.TryGetValue(memberInfo, out var cultureInfo))
                serializedValue = ((dynamic)value).ToString(cultureInfo);
            else
                serializedValue = (PrintToString(value, nestingLevel + 1));

            if (serializationConfig.MemberTrimToLength.TryGetValue(memberInfo, out var trimRule))
                return trimRule(serializedValue);

            if (serializationConfig.TypeTrimToLength.TryGetValue(GetMemberInfoType(memberInfo),
                    out var typeTrimToLength))
                return typeTrimToLength(serializedValue);
            return serializedValue;
        }

        private static Type GetMemberInfoType(MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)memberInfo).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Method => ((MethodInfo)memberInfo).ReturnType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => throw new ArgumentException(
                    "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo")
            };
        }

        private static bool IsFieldOrProperty(MemberInfo memberInfo) =>
            memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property;

        private static object GetValue(object obj, MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => (memberInfo as FieldInfo)?.GetValue(obj),
                MemberTypes.Property => (memberInfo as PropertyInfo)?.GetValue(obj),
                _ => null
            };
        }
    }
}