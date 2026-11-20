using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        internal const BindingFlags PublicInstanceFlag = BindingFlags.Instance | BindingFlags.Public;
        private readonly HashSet<PropertyInfo> _excludedProperties;
        private readonly HashSet<FieldInfo> _excludedFields;
        internal readonly Dictionary<MemberInfo, Func<object, string>> CustomSerializers;

        public PrintingConfig()
        {
            _excludedProperties = new();
            _excludedFields = new();
            CustomSerializers = new();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var targetProperties = typeof(TOwner).GetProperties(PublicInstanceFlag).Where(p => p.PropertyType == typeof(TPropType));
            var targetFields = typeof(TOwner).GetFields(PublicInstanceFlag).Where(p => p.FieldType == typeof(TPropType));
            return new PropertyPrintingConfig<TOwner, TPropType>(this, [.. targetProperties, .. targetFields]);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var member = GetMemberExpression(memberSelector);

            switch (member.Member)
            {
                case PropertyInfo:
                case FieldInfo:
                    return new(this, [member.Member]);

                default:
                    throw new ArgumentException(
                        $"Member '{member.Member.Name}' is not a property or field.",
                        nameof(memberSelector));
            }
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            ArgumentNullException.ThrowIfNull(memberSelector);

            var member = GetMemberExpression(memberSelector);

            switch (member.Member)
            {
                case PropertyInfo prop:
                    _excludedProperties.Add(prop);
                    break;

                case FieldInfo field:
                    _excludedFields.Add(field);
                    break;

                default:
                    throw new ArgumentException(
                        $"Member '{member.Member.Name}' is not a property or field.",
                        nameof(memberSelector));
            }

            return this;
        }

        internal static MemberExpression GetMemberExpression<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = memberSelector.Body;

            while (body is UnaryExpression u &&
                   (u.NodeType == ExpressionType.Convert ||
                    u.NodeType == ExpressionType.ConvertChecked))
            {
                body = u.Operand;
            }

            if (body is not MemberExpression member ||
                member.Expression is not ParameterExpression)
            {
                throw new ArgumentException(
                    "Selector must point directly to a property or field of the owner, e.g. x => x.Name.",
                    nameof(memberSelector));
            }

            return member;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedProperties.AddRange(typeof(TOwner).GetProperties().Where(p => p.PropertyType == typeof(TPropType)));
            _excludedFields.AddRange(typeof(TOwner).GetFields().Where(f => f.FieldType == typeof(TPropType)));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            var overridenToStringMethods = type.GetMethods().Where(m => m.Name == "ToString" && m.GetParameters().Length == 0);
            var toStringData =
                overridenToStringMethods.Any() && overridenToStringMethods.Single().DeclaringType == type
                ? $" {obj}" : string.Empty;
            sb.AppendLine(type.Name + toStringData);
            sb.Append(MembersToStringBuilder(nestingLevel, type.GetProperties(PublicInstanceFlag), _excludedProperties, m => m.GetValue(obj)));
            sb.Append(MembersToStringBuilder(nestingLevel, type.GetFields(PublicInstanceFlag), _excludedFields, m => m.GetValue(obj)));
            return sb.ToString();
        }

        private StringBuilder MembersToStringBuilder<TMemberType>(
            int nestingLevel,
            IEnumerable<TMemberType> members,
            ICollection<TMemberType> excludedMembers,
            Func<TMemberType, object?> getObj)
            where TMemberType : MemberInfo
        {
            var identation = new string('\t', nestingLevel + 1);
            StringBuilder sb = new();
            foreach (var memberInfo in members)
            {
                var hasCustomSerializer = CustomSerializers.TryGetValue(memberInfo, out var customSerializer);
                var obj = getObj(memberInfo);
                var memberInfoString = hasCustomSerializer
                    ? customSerializer(obj) + Environment.NewLine
                    : PrintToString(obj, nestingLevel + 1);
                if (excludedMembers.Contains(memberInfo)) continue;
                sb.Append(identation + memberInfo.Name + " = " +
                          memberInfoString);
            }
            return sb;
        }
    }
}