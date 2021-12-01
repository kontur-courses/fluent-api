using System;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
            => ObjectPrinter.For<T>()
                .PrintToString(obj);

        public static string PrintToString<T>
            (this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configureSettings)
            => configureSettings.Invoke(new PrintingConfig<T>())
                .PrintToString(obj);
    }
}