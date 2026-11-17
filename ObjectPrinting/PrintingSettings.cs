using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    internal sealed class PrintingSettings
    {
        private readonly HashSet<Type> excludedTypes = [];
        private readonly Dictionary<Type, TypeSettings> typeSettings = [];
        private readonly Dictionary<MemberInfo, MemberSettings> memberSettings = [];

        internal void ExcludeType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            excludedTypes.Add(NormalizeType(type));
        }

        internal bool IsTypeExcluded(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return excludedTypes.Contains(NormalizeType(type));
        }

        internal void ExcludeMember(MemberInfo member)
        {
            GetMemberSettings(member).IsExcluded = true;
        }

        internal bool IsMemberExcluded(MemberInfo member)
        {
            return memberSettings.TryGetValue(member, out var settings)
                   && settings.IsExcluded;
        }

        internal void SetTypeFormatter(
            Type type,
            Func<object?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            var settings = GetTypeSettings(type);
            settings.Formatter = formatter;
            settings.Culture = null;
        }

        internal Func<object?, string>? GetTypeFormatter(Type type)
        {
            return typeSettings.TryGetValue(
                NormalizeType(type),
                out var settings)
                ? settings.Formatter
                : null;
        }

        internal void SetTypeCulture(Type type, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);

            var settings = GetTypeSettings(type);
            settings.Culture = CopyCulture(culture);
            settings.Formatter = null;
        }

        internal CultureInfo? GetTypeCulture(Type type)
        {
            return typeSettings.TryGetValue(
                NormalizeType(type),
                out var settings)
                ? settings.Culture
                : null;
        }

        internal void SetMemberFormatter(
            MemberInfo member,
            Func<object?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            var settings = GetMemberSettings(member);
            settings.Formatter = formatter;
            settings.Culture = null;
        }

        internal Func<object?, string>? GetMemberFormatter(MemberInfo member)
        {
            return memberSettings.TryGetValue(member, out var settings)
                ? settings.Formatter
                : null;
        }

        internal void SetMemberCulture(MemberInfo member, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);

            var settings = GetMemberSettings(member);
            settings.Culture = CopyCulture(culture);
            settings.Formatter = null;
        }

        internal CultureInfo? GetMemberCulture(MemberInfo member)
        {
            return memberSettings.TryGetValue(member, out var settings)
                ? settings.Culture
                : null;
        }

        internal void SetMemberMaxLength(MemberInfo member, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            GetMemberSettings(member).MaxLength = maxLength;
        }

        internal int? GetMemberMaxLength(MemberInfo member)
        {
            return memberSettings.TryGetValue(member, out var settings)
                ? settings.MaxLength
                : null;
        }

        private TypeSettings GetTypeSettings(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            type = NormalizeType(type);

            if (!typeSettings.TryGetValue(type, out var settings))
            {
                settings = new TypeSettings();
                typeSettings.Add(type, settings);
            }

            return settings;
        }

        private MemberSettings GetMemberSettings(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member);

            if (!memberSettings.TryGetValue(member, out var settings))
            {
                settings = new MemberSettings();
                memberSettings.Add(member, settings);
            }

            return settings;
        }

        private static Type NormalizeType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static CultureInfo CopyCulture(CultureInfo culture)
        {
            return CultureInfo.ReadOnly((CultureInfo)culture.Clone());
        }

        private sealed class TypeSettings
        {
            internal Func<object?, string>? Formatter { get; set; }

            internal CultureInfo? Culture { get; set; }
        }

        private sealed class MemberSettings
        {
            internal bool IsExcluded { get; set; }

            internal Func<object?, string>? Formatter { get; set; }

            internal CultureInfo? Culture { get; set; }

            internal int? MaxLength { get; set; }
        }
    }
}