using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Func<object, string>> typePrintingRules =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> memberPrintingRules =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<MemberInfo, CultureInfo> memberCultures = new Dictionary<MemberInfo, CultureInfo>();

        private readonly Dictionary<MemberInfo, Func<string, string>> memberTrimToLength =
            new Dictionary<MemberInfo, Func<string, string>>();

        private Func<string, string> globalTrimToLength = x => x;
        private MemberInfo currentMember;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentMember = ((MemberExpression) memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentMember = ((MemberExpression) memberSelector.Body).Member;
            excludedMembers.Add(currentMember);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public void SetPrintingRule<TPropType>(Func<TPropType, string> print)
        {
            if (currentMember == null)
                typePrintingRules[typeof(TPropType)] = x => print((TPropType) x);
            else
                memberPrintingRules[currentMember] = x => print((TPropType) x);
            currentMember = null;
        }

        public void SetCulture<TPropType>(CultureInfo culture)
        {
            if (currentMember == null)
                typeCultures[typeof(TPropType)] = culture;
            else
                memberCultures[currentMember] = culture;
            currentMember = null;
        }

        public void SetTrim(int maxLen)
        {
            if (currentMember == null)
            {
                globalTrimToLength = x => x.Substring(0, Math.Min(x.Length, maxLen));
            }
            else
            {
                memberTrimToLength[currentMember] = x => x.Substring(0, Math.Min(x.Length, maxLen));
            }

            currentMember = null;
        }


        private string PrintToString(object obj, int nestingLevel)
        {
            if (nestingLevel > 100)
                return "";

            if (obj == null)
                return "null";


            if (typePrintingRules.TryGetValue(obj.GetType(), out var typePrintingRule))
                return typePrintingRule.DynamicInvoke(obj)?.ToString();

            if (finalTypes.Contains(obj.GetType()))
            {
                if (typeCultures.TryGetValue(obj.GetType(), out var cultureInfo))
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
            var identation = new string('\t', nestingLevel + 1);
            sb.AppendLine("{");
            foreach (var key in dictionary.Keys)
            {
                sb.Append(identation + "{" + PrintToString(key, nestingLevel + 1) + Environment.NewLine
                          + identation + ":" + PrintToString(dictionary[key], nestingLevel + 1) + "},"
                          + Environment.NewLine);
            }

            sb.Append(identation + "}");
            return sb.ToString();
        }

        private string SerializeIEnumerable(IEnumerable obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("[");
            foreach (var key in obj)
                stringBuilder.AppendLine(identation + PrintToString(key, nestingLevel + 1) + ",");

            stringBuilder.Append(identation + "]");
            var serializedIEnumerable = stringBuilder.ToString();

            var lastIndexOf = serializedIEnumerable.LastIndexOf(",", StringComparison.Ordinal);
            if (lastIndexOf != -1)
                return serializedIEnumerable.Remove(lastIndexOf, 1);
            return serializedIEnumerable;
        }

        private bool IsExcludedMember(MemberInfo memberInfo)
        {
            if ((memberInfo.MemberType & MemberTypes.Field) != 0)
                return excludedTypes.Contains((memberInfo as FieldInfo)?.FieldType) ||
                       excludedMembers.Contains(memberInfo);

            if ((memberInfo.MemberType & MemberTypes.Property) != 0)
                return excludedTypes.Contains((memberInfo as PropertyInfo)?.PropertyType) ||
                       excludedMembers.Contains(memberInfo);

            return false;
        }


        private string SerializeMembers(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            foreach (var memberInfo in type.GetMembers()
                .Where(IsFieldOrProperty).Where(x => !IsExcludedMember(x)))
            {
                var value = GetValue(obj, memberInfo);

                if (value != null && value.Equals(obj))
                    continue;

                sb.Append(Environment.NewLine + identation + memberInfo.Name + " = " +
                          SerializeValue(nestingLevel, memberInfo, value));
            }

            return sb.ToString();
        }

        private string SerializeValue(int nestingLevel, MemberInfo memberInfo, object value)
        {
            var sb = "";

            if (memberPrintingRules.TryGetValue(memberInfo, out var rule))
                sb = rule.DynamicInvoke(value)?.ToString();
            else if (memberCultures.TryGetValue(memberInfo, out var cultureInfo))
                sb = ((dynamic) value).ToString(cultureInfo);
            else
                sb = (PrintToString(value, nestingLevel + 1));

            sb = globalTrimToLength(sb);

            if (memberTrimToLength.TryGetValue(memberInfo, out var trimRule))
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