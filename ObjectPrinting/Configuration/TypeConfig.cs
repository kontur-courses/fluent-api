using System;
using System.Globalization;

namespace ObjectPrinting
{
    /// <summary>Настраивает сериализатор и культуру для значений указанного типа</summary>
    /// <typeparam name="TOwner">Тип печатаемого объекта</typeparam>
    /// <typeparam name="TValue">Тип настраиваемых значений</typeparam>
    public sealed class TypeConfig<TOwner, TValue>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PrintingSettings settings;

        /// <summary>Связывает настройку типа с общей конфигурацией печати</summary>
        internal TypeConfig(
            PrintingConfig<TOwner> printingConfig,
            PrintingSettings settings)
        {
            this.printingConfig = printingConfig;
            this.settings = settings;
        }

        /// <summary>Задаёт преобразование в строку для значений этого фактического типа</summary>
        /// <param name="serializer">Функция, которая получает значение и возвращает строку</param>
        /// <returns>Основная конфигурация для продолжения цепочки</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="serializer"/> равен <see langword="null"/></exception>
        public PrintingConfig<TOwner> Using(
            Func<TValue, string> serializer)
        {
            settings.SetTypeSerializer(serializer);
            return printingConfig;
        }

        /// <summary>
        /// Задаёт культуру для значений типа, реализующего <see cref="IFormattable"/>
        /// Сериализатор типа, если он задан, имеет приоритет
        /// </summary>
        /// <param name="culture">Культура для форматирования значений</param>
        /// <returns>Основная конфигурация для продолжения цепочки</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="culture"/> равен <see langword="null"/></exception>
        public PrintingConfig<TOwner> WithCulture(
            CultureInfo culture)
        {
            settings.SetTypeCulture<TValue>(culture);
            return printingConfig;
        }
    }
}
