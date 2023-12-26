using System;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }
    }

    public static class ObjectPrinterExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
    }
}