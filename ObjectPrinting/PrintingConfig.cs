using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private readonly HashSet<object> excludedTypes = new();
        private readonly Dictionary<Type, Func<object?, string>> typeSerializers = new();
        private readonly Dictionary<Type, CultureInfo> typeCultures = new();
        private readonly Dictionary<MemberInfo, Func<object?, string>> memberSerializers = new();
        private readonly Dictionary<MemberInfo, int> memberStringLimits = new();

        public PrintingConfig<TOwner> Excluding<TValue>()
        {
            excludedTypes.Add(typeof(TValue));
            return this;
        }

        public TypeConfig<TOwner, TValue> Configure<TValue>()
        {
            return new TypeConfig<TOwner, TValue>(this);
        }

        public MemberConfig<TOwner, TValue> Configure<TValue>(
            Expression<Func<TOwner, TValue>> expression)
        {
            return new MemberConfig<TOwner, TValue>(
                this,
                GetSelectedMember(expression));
        }

        public StringMemberConfig<TOwner> Configure(
            Expression<Func<TOwner, string>> expression)
        {
            return new StringMemberConfig<TOwner>(
                this,
                GetSelectedMember(expression));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (typeSerializers.TryGetValue(obj.GetType(), out var serializer))
                return serializer(obj) + Environment.NewLine;

            if (typeCultures.TryGetValue(obj.GetType(), out var culture) && obj is IFormattable formattable)
                return formattable.ToString(null, culture) + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var member in GetPublicMembers(type))
            {
                if (excludedTypes.Contains(GetMemberType(member)))
                    continue;

                var value = member switch
                {
                    PropertyInfo property => property.GetValue(obj),
                    FieldInfo field => field.GetValue(obj),
                    _ => null
                };

                if (value is string text
                && memberStringLimits.TryGetValue(member, out var maxLength)
                && text.Length > maxLength)
                {
                    value = text[..maxLength];
                }

                sb.Append(indentation)
                    .Append(member.Name)
                    .Append(" = ");
                if (memberSerializers.TryGetValue(member, out var memberSerializer))
                {
                    sb.Append(memberSerializer(value))
                        .Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(PrintToString(value, nestingLevel + 1));
                }
            }
            return sb.ToString();
        }

        private static IEnumerable<MemberInfo> GetPublicMembers(Type type)
        {
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            return properties.Cast<MemberInfo>().Concat(fields);
        }

        private static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new ArgumentException("Неизвестный тип")
            };
        }

        internal void SetTypeSerializer<TValue>(Func<TValue, string> serializer)
        {
            ArgumentNullException.ThrowIfNull(serializer);

            typeSerializers[typeof(TValue)] =
                value => serializer((TValue)value!);
        }

        internal void SetTypeCulture<TValue>(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);
            typeCultures[typeof(TValue)] = culture;
        }

        internal void SetMemberSerializer<TValue>(
            MemberInfo member,
            Func<TValue, string> serializer)
        {
            ArgumentNullException.ThrowIfNull(serializer);

            memberSerializers[member] =
                value => serializer((TValue)value!);
        }

        internal void SetMemberStringLimit(MemberInfo member, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            memberStringLimits[member] = maxLength;
        }

        private static MemberInfo GetSelectedMember<TValue>(
    Expression<Func<TOwner, TValue>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            if (expression.Body is MemberExpression selectedMember
                && selectedMember.Expression == expression.Parameters[0]
                && (selectedMember.Member is PropertyInfo
                    || selectedMember.Member is FieldInfo))
            {
                return selectedMember.Member;
            }

            throw new ArgumentException(
                "Ожидался выбор свойства или поля на входе",
                nameof(expression));
        }

    }
}
