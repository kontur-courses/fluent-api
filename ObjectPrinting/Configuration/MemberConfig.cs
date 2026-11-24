using System;
using System.Reflection;

namespace ObjectPrinting
{
    /// <summary>Настраивает печать выбранного свойства или поля</summary>
    /// <typeparam name="TOwner">Тип печатаемого объекта</typeparam>
    /// <typeparam name="TValue">Тип выбранного свойства или поля</typeparam>
    public class MemberConfig<TOwner, TValue>
    {
        protected readonly PrintingConfig<TOwner> PrintingConfig;
        private protected readonly PrintingSettings Settings;
        protected readonly MemberInfo MemberInfo;

        /// <summary>Связывает выбранное свойство или поле с общей конфигурацией печати</summary>
        internal MemberConfig(
            PrintingConfig<TOwner> printingConfig,
            PrintingSettings settings,
            MemberInfo memberInfo)
        {
            PrintingConfig = printingConfig;
            Settings = settings;
            MemberInfo = memberInfo;
        }

        /// <summary>Задаёт сериализатор для выбранного свойства или поля</summary>
        /// <param name="serializer">Функция, которая получает значение и возвращает строку</param>
        /// <returns>Основная конфигурация для продолжения цепочки</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="serializer"/> равен <see langword="null"/></exception>
        public PrintingConfig<TOwner> Using(
            Func<TValue, string> serializer)
        {
            Settings.SetMemberSerializer(MemberInfo, serializer);
            return PrintingConfig;
        }
    }

    /// <summary>Настраивает печать и обрезку выбранного строкового свойства или поля</summary>
    /// <typeparam name="TOwner">Тип печатаемого объекта</typeparam>
    public sealed class StringMemberConfig<TOwner>
        : MemberConfig<TOwner, string>
    {
        /// <summary>Связывает выбранное строковое свойство или поле с общей конфигурацией печати</summary>
        internal StringMemberConfig(
            PrintingConfig<TOwner> printingConfig,
            PrintingSettings settings,
            MemberInfo memberInfo)
            : base(printingConfig, settings, memberInfo)
        {
        }

        /// <summary>Ограничивает длину выбранной строки перед её сериализацией</summary>
        /// <param name="maxLength">Максимальная длина строки. Ноль оставляет пустую строку</param>
        /// <returns>Основная конфигурация для продолжения цепочки</returns>
        /// <exception cref="ArgumentOutOfRangeException">Параметр <paramref name="maxLength"/> меньше нуля</exception>
        public PrintingConfig<TOwner> Truncate(int maxLength)
        {
            Settings.SetMemberStringLimit(MemberInfo, maxLength);
            return PrintingConfig;
        }
    }
}
