using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj, int nestingLevel)
        {
            return ObjectPrinter.For<T>().PrintToString(obj, nestingLevel);
        }
    }
}
