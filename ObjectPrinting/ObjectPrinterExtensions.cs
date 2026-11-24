using System;

namespace ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        public static string Print<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }


        public static string Print<T>(this T obj, Action<PrintingConfig<T>> configure)
        {
            var config = ObjectPrinter.For<T>();
            configure(config);
            return config.PrintToString(obj);
        }

        public static PrintingConfig<TOwner> TrimToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> config,
            int maxLength)
        {
            
            return config.Using(s =>
                            s.Length <= maxLength ?
                            s :
                            s.Substring(0, maxLength));
        }
    }
}

