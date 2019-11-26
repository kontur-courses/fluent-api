using System;
using System.Collections;
using System.Linq;
using FluentAssertions.Common;

namespace ObjectPrintingHomeTask.ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            if (!obj.GetType().Implements(typeof(ICollection)))
                return ObjectPrinter.For<T>().PrintToString(obj);
            var stringToPrint = typeof(T).Name + Environment.NewLine;
            return ((ICollection)obj).Cast<object>()
                                      .Aggregate(stringToPrint, (current, e) => current + e.PrintToString());
        }
    }
}
