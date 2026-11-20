using System;
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
        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private readonly Dictionary<Type, TypeSettings> typeSettings = new();
        private readonly Dictionary<MemberInfo, MemberSettings> memberSettings = new();

        public PrintingConfig<TOwner> Excluding<TValue>()
        {
            SetExcludingType<TValue>();
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TValue>(Expression<Func<TOwner, TValue>> expression)
        {
            SetExcludingMember(GetSelectedMember(expression));
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

            var type = obj.GetType();
            if (typeSettings.TryGetValue(type, out var settings))
            {
                var typeSerializer = settings.Serializer;
                if (typeSerializer is not null)
                    return typeSerializer(obj) + Environment.NewLine;

                var culture = settings.Culture;
                if (culture is not null && obj is IFormattable formattable)
                    return formattable.ToString(null, culture) + Environment.NewLine;
            }

            if (FinalTypes.Contains(type))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var member in GetPublicMembers(type))
            {
                var memberType = GetMemberType(member);
                memberSettings.TryGetValue(member, out var memberConfiguration);

                if ((typeSettings.TryGetValue(memberType, out var typeConfiguration)
                        && typeConfiguration.IsExcluded)
                    || memberConfiguration?.IsExcluded == true)
                {
                    continue;
                }

                var value = member switch
                {
                    PropertyInfo property => property.GetValue(obj),
                    FieldInfo field => field.GetValue(obj),
                    _ => throw new ArgumentException("Unknown member type.", nameof(member))
                };

                if (value is string text
                    && memberConfiguration?.MaxStringLength is int maxLength
                    && text.Length > maxLength)
                {
                    value = text[..maxLength];
                }

                sb.Append(indentation)
                    .Append(member.Name)
                    .Append(" = ");

                var memberSerializer = memberConfiguration?.Serializer;
                if (memberSerializer is not null)
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

            Func<object?, string> objectSerializer = value => serializer((TValue)value!);
            UpdateTypeSettings(
                typeof(TValue),
                settings => settings with { Serializer = objectSerializer });
        }

        internal void SetTypeCulture<TValue>(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);
            UpdateTypeSettings(
                typeof(TValue),
                settings => settings with { Culture = culture });
        }

        internal void SetExcludingType<TValue>()
        {
            UpdateTypeSettings(
                typeof(TValue),
                settings => settings with { IsExcluded = true });
        }

        internal void SetMemberSerializer<TValue>(
            MemberInfo member,
            Func<TValue, string> serializer)
        {
            ArgumentNullException.ThrowIfNull(serializer);

            Func<object?, string> objectSerializer = value => serializer((TValue)value!);
            UpdateMemberSettings(
                member,
                settings => settings with { Serializer = objectSerializer });
        }

        internal void SetMemberStringLimit(MemberInfo member, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            UpdateMemberSettings(
                member,
                settings => settings with { MaxStringLength = maxLength });
        }

        internal void SetExcludingMember(MemberInfo member)
        {
            UpdateMemberSettings(
                member,
                settings => settings with { IsExcluded = true });
        }

        private void UpdateTypeSettings(Type type, Func<TypeSettings, TypeSettings> update)
        {
            var current = typeSettings.TryGetValue(type, out var settings)
                ? settings
                : new TypeSettings();

            typeSettings[type] = update(current);
        }

        private void UpdateMemberSettings(
            MemberInfo member,
            Func<MemberSettings, MemberSettings> update)
        {
            var current = memberSettings.TryGetValue(member, out var settings)
                ? settings
                : new MemberSettings();

            memberSettings[member] = update(current);
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

        private sealed record TypeSettings(
            bool IsExcluded = false,
            Func<object?, string>? Serializer = null,
            CultureInfo? Culture = null);

        private sealed record MemberSettings(
            bool IsExcluded = false,
            Func<object?, string>? Serializer = null,
            int? MaxStringLength = null);

    }
}
