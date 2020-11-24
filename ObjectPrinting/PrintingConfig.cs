using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> typesToSerialize = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
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

            if (typesToSerialize.Contains(obj.GetType()))
                return SerializeValue(obj, info);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            var all = type.GetMembers();
            var check = GetMembersToSerialize(type).ToList();

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

            if (AlternativeSerializersForMembers.ContainsKey(info))
                return AlternativeSerializersForMembers[info].DynamicInvoke(obj) + Environment.NewLine;

            if (AlternativeSerializersForTypes.ContainsKey(type))
                return AlternativeSerializersForTypes[type].DynamicInvoke(obj) + Environment.NewLine;

            return obj + Environment.NewLine;
        }

        private IEnumerable<MemberInfo> GetMembersToSerialize(Type type)
        {
            foreach (var member in type.GetMembers())
            {
                if (membersToExclude.Contains(member))
                    continue;

                if (member.MemberType is MemberTypes.Field)
                {
                    var field = (FieldInfo) member;
                    if (!typesToExclude.Contains(field.FieldType))
                        yield return field;
                }

                if (member.MemberType is MemberTypes.Property)
                {
                    var prop = (PropertyInfo)member;
                    if (!typesToExclude.Contains(prop.PropertyType))
                        yield return prop;
                }
            }
        }

        public string PrintToString(TOwner obj, int maxNestingDepth = 0)
        {
            return PrintToString(obj, 0, maxNestingDepth, null);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
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