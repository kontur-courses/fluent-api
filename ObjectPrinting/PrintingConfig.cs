using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> typesToDefaultSerialize = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(bool),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();
        private readonly HashSet<MemberInfo> membersToExclude = new HashSet<MemberInfo>();

        internal Dictionary<Type, Delegate> AlternativeSerializersForTypes = new Dictionary<Type, Delegate>();
        internal Dictionary<MemberInfo, Delegate> AlternativeSerializersForMembers =
            new Dictionary<MemberInfo, Delegate>(); 

        private string PrintToString(object obj, int nestingLevel, int maxNestingDepth, MemberInfo info)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (typesToDefaultSerialize.Contains(obj.GetType()))
                return SerializeValue(obj, info);

            if (obj is IEnumerable enumerable)
                return SerializeEnumerable(enumerable, nestingLevel, maxNestingDepth);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var member in GetMembersToSerialize(type))
            {
                var value = member.MemberType == MemberTypes.Property
                    ? (member as PropertyInfo)?.GetValue(obj)
                    : (member as FieldInfo)?.GetValue(obj);

                var serializedValue = (nestingLevel + 1 > maxNestingDepth && type == value?.GetType())
                    ? "Reached maximal level of nesting" + Environment.NewLine
                    : $"{PrintToString(value, nestingLevel + 1, maxNestingDepth, member)}";

                sb.Append($"{identation}{member.Name} = {serializedValue}");
            }

            return sb.ToString();
        }

        private string SerializeValue(object obj, MemberInfo info)
        {
            var type = obj.GetType();

            if (info != null && AlternativeSerializersForMembers.ContainsKey(info))
                return AlternativeSerializersForMembers[info].DynamicInvoke(obj) + Environment.NewLine;

            if (AlternativeSerializersForTypes.ContainsKey(type))
                return AlternativeSerializersForTypes[type].DynamicInvoke(obj) + Environment.NewLine;

            return obj + Environment.NewLine;
        }

        private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel, int maxNestingDepth)
        {
            var sb = new StringBuilder($"{enumerable.GetType().Name}\r\n");
            var identation = new string('\t', nestingLevel + 1);
            sb.Append($"{new string('\t', nestingLevel)}{{\r\n");
            var startLength = sb.Length;

            foreach (var obj in enumerable)
                sb.Append($"{identation}{PrintToString(obj, nestingLevel + 1, maxNestingDepth, null)}");

            if (sb.Length == startLength)
                sb.Append("Empty");
            sb.Append($"{new string('\t', nestingLevel)}}}\r\n");
            
            return sb.ToString();
        }

        private IEnumerable<MemberInfo> GetMembersToSerialize(Type type)
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (membersToExclude.Contains(member))
                    continue;

                if (member is FieldInfo fi)
                    if (!typesToExclude.Contains(fi.FieldType))
                        yield return fi;

                if(member is PropertyInfo pi)
                    if (!typesToExclude.Contains(pi.PropertyType))
                        yield return pi;
            }
        }

        public string PrintToString(TOwner obj, int maxNestingDepth = 5)
        {
            return PrintToString(obj, 0, maxNestingDepth, null);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this);
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new MemberPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            membersToExclude.Add(((MemberExpression) memberSelector.Body).Member);
            return this;
        }
    }
}