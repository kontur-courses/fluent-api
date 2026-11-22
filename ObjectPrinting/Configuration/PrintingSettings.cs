using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    internal sealed class PrintingSettings
    {
        private readonly Dictionary<Type, TypeSettings> typeSettings = new();
        private readonly Dictionary<MemberInfo, MemberSettings> memberSettings = new();

        internal TypeSettings? GetTypeSettings(Type type)
        {
            return typeSettings.TryGetValue(type, out var settings) ? settings : null;
        }

        internal MemberSettings? GetMemberSettings(MemberInfo member)
        {
            return memberSettings.TryGetValue(member, out var settings) ? settings : null;
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
