using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> configureSetting)
        {
            return configureSetting(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}
