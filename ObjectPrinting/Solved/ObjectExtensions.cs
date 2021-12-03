using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj, int maxNestingLevel)
        {
            return ObjectPrinter.For<T>().PrintToString(obj, maxNestingLevel);
        }
    }
}