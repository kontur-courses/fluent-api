using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, TypeSettings> typeSettings = new();
        private readonly Dictionary<MemberInfo, MemberSettings> memberSettings = new();
        private readonly ObjectWriter objectWriter;

        public PrintingConfig()
        {
            objectWriter = new ObjectWriter(typeSettings, memberSettings);
        }

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
            return objectWriter.Print(obj);
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
    }

    internal sealed record TypeSettings(
        bool IsExcluded = false,
        Func<object?, string>? Serializer = null,
        CultureInfo? Culture = null);

    internal sealed record MemberSettings(
        bool IsExcluded = false,
        Func<object?, string>? Serializer = null,
        int? MaxStringLength = null);
}
