using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    internal static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Action<PrintingConfig<T>> config)
        {
            var printer = ObjectPrinter.For<T>();
            config(printer);
            return printer.PrintToString(obj);
        }
    }
}
