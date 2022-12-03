using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObjectPrinting.Common
{
    public static class PrintingConfigExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string PrintToString<T>(this T obj, Func<IPrintingConfig<T>, IPrintingConfig<T>> config)
        {
            return config(ObjectPrinter.For<T>()).PrintToString(obj);
        }
    }
}