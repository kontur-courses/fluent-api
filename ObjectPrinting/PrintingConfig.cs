using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<MemberInfo> excludedMembers = [];

        private readonly Dictionary<Type, Func<object, string>> formattersByType = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> formattersByMember = new();


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMember(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedMembers.Add(GetMember(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var path = new HashSet<object>(ReferenceEqualityComparer.Instance);

            return PrintToString(obj, 0, path);
        }

        private string PrintToString(object? obj, int nestingLevel, HashSet<object> path)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (IsFinalType(type))
                return obj + Environment.NewLine;

            if (!path.Add(obj))
                return "<Cycle>" + Environment.NewLine;


            if (obj is IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().Select(x => PrintToString(x, nestingLevel + 1, path));
                return "[ " + string.Join(", ", items) + " ]";
            }


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);

            const BindingFlags publicInstanceFlags = BindingFlags.Public | BindingFlags.Instance;

            foreach (var propertyInfo in type.GetProperties(publicInstanceFlags))
            {
                if (IsPublicProperty(propertyInfo))
                {
                    AppendMember(propertyInfo, propertyInfo.PropertyType, () => propertyInfo.GetValue(obj));
                }
            }

            foreach (var fieldInfo in type.GetFields(publicInstanceFlags))
            {
                AppendMember(fieldInfo, fieldInfo.FieldType, () => fieldInfo.GetValue(obj));
            }


            path.Remove(obj);
            return sb.ToString();

            void AppendMember(MemberInfo member, Type memberType, Func<object?> readValue)
            {
                if (excludedMembers.Contains(member) || excludedTypes.Contains(memberType))
                    return;

                var value = readValue();
                string printedValue;

                if (value is not null && TryGetFormatter(member, memberType, out var formatter))
                    printedValue = formatter(value) + Environment.NewLine;
                else
                    printedValue = PrintToString(value, nestingLevel + 1, path);

                sb.Append(identation + member.Name + " = " + printedValue);
            }
        }

        private static bool IsPublicProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod is { IsPublic: true } && propertyInfo.GetIndexParameters().Length == 0;
        }

        private bool TryGetFormatter(
            MemberInfo member,
            Type memberType,
            [NotNullWhen(true)] out Func<object, string>? formatter)
        {
            return formattersByMember.TryGetValue(member, out formatter)
                   || formattersByType.TryGetValue(memberType, out formatter);
        }


        private static MemberInfo GetMember<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            selector.Body is MemberExpression { Member: var member and (PropertyInfo or FieldInfo) }
                ? member
                : throw new ArgumentException("selector expression must be a member expression");

        internal void SetFormatter(Type type, MemberInfo? member, Func<object, string> formatter)
        {
            if (member == null) formattersByType[type] = formatter;
            else formattersByMember[member] = formatter;
        }

        private static bool IsFinalType(Type type) =>
            type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan);
    }
}