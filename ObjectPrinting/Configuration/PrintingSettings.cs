using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    /// <summary>Хранит правила печати для типов, отдельных свойств и полей</summary>
    internal sealed class PrintingSettings
    {
        private readonly Dictionary<Type, TypeSettings> typeSettings = new();
        private readonly Dictionary<MemberInfo, MemberSettings> memberSettings = new();

        /// <summary>Возвращает правила по точному типу или <see langword="null"/>, если их нет</summary>
        internal TypeSettings? GetTypeSettings(Type type)
        {
            return typeSettings.TryGetValue(type, out var settings) ? settings : null;
        }

        /// <summary>Возвращает правила выбранного свойства или поля либо <see langword="null"/>, если их нет</summary>
        internal MemberSettings? GetMemberSettings(MemberInfo member)
        {
            return memberSettings.TryGetValue(member, out var settings) ? settings : null;
        }

        /// <summary>Сохраняет сериализатор для указанного типа</summary>
        internal void SetTypeSerializer<TValue>(Func<TValue, string> serializer)
        {
            ArgumentNullException.ThrowIfNull(serializer);

            Func<object?, string> objectSerializer = value => serializer((TValue)value!);
            UpdateTypeSettings(
                typeof(TValue),
                settings => settings with { Serializer = objectSerializer });
        }

        /// <summary>Сохраняет культуру форматирования для указанного типа</summary>
        internal void SetTypeCulture<TValue>(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);
            UpdateTypeSettings(
                typeof(TValue),
                settings => settings with { Culture = culture });
        }

        /// <summary>Помечает свойства и поля указанного типа для исключения из вывода</summary>
        internal void SetExcludingType<TValue>()
        {
            UpdateTypeSettings(
                typeof(TValue),
                settings => settings with { IsExcluded = true });
        }

        /// <summary>Сохраняет сериализатор для выбранного свойства или поля</summary>
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

        /// <summary>Сохраняет допустимый предел длины строки для выбранного свойства или поля</summary>
        internal void SetMemberStringLimit(MemberInfo member, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            UpdateMemberSettings(
                member,
                settings => settings with { MaxStringLength = maxLength });
        }

        /// <summary>Помечает выбранное свойство или поле для исключения из вывода</summary>
        internal void SetExcludingMember(MemberInfo member)
        {
            UpdateMemberSettings(
                member,
                settings => settings with { IsExcluded = true });
        }

        /// <summary>Обновляет текущие правила типа или создаёт запись при первой настройке</summary>
        private void UpdateTypeSettings(Type type, Func<TypeSettings, TypeSettings> update)
        {
            var current = typeSettings.TryGetValue(type, out var settings)
                ? settings
                : new TypeSettings();

            typeSettings[type] = update(current);
        }

        /// <summary>Обновляет правила свойства или поля либо создаёт запись при первой настройке</summary>
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

    /// <summary>Хранит исключение, сериализатор и культуру для одного типа</summary>
    internal sealed record TypeSettings(
        bool IsExcluded = false,
        Func<object?, string>? Serializer = null,
        CultureInfo? Culture = null);

    /// <summary>Хранит исключение, сериализатор и предел длины для одного свойства или поля</summary>
    internal sealed record MemberSettings(
        bool IsExcluded = false,
        Func<object?, string>? Serializer = null,
        int? MaxStringLength = null);
}
