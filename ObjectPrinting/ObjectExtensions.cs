using System;

namespace ObjectPrinting;

public static class ObjectExtensions
{
    /// <summary>
    ///  Сериализует объект в строку с указанной конфигурацией
    /// </summary>
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }

    /// <summary>
    ///  Сериализует объект в строку с конфигурацией по умолчанию
    /// </summary>
    public static string PrintToString<T>(this T obj)
    {
        return obj.PrintToString(c => c);
    }
}
