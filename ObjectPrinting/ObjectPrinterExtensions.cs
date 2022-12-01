using System;

namespace ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        /// <summary>
        /// Метод, который позволяет сериализовать объект, используя настройки сериализации по умолчанию
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <typeparam name="T">Тип, который необходимо сериализовать</typeparam>
        /// <returns>Строковое представление объекта</returns>
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        
        /// <summary>
        /// Метод, который позволяет сериализовать объект, используя настроки по умолчанию.
        /// Но можно передать уточняющие параметры сериализации
        /// </summary>
        /// <param name="obj">Объект, который нужно сериализовать</param>
        /// <param name="config">Уточняющие настройки для сериализации</param>
        /// <typeparam name="T">Тип, для которого строится строковое представление</typeparam>
        /// <returns>Стрококвое представление объекта</returns>
        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}