using System;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        public static string Serialize<T>(this T target) =>
            PrintingConfig<T>.Default.PrintToString(target);

        public static string Serialize<T>(this T target, Action<PrintingConfig<T>> configurator)
        {
            var printingConfig = For<T>();
            configurator.Invoke(printingConfig);
            return printingConfig.PrintToString(target);
        }
    }
}