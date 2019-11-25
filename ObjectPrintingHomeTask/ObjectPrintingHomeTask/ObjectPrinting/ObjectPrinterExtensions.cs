using System;
using System.Collections;
using System.Linq;
using FluentAssertions.Common;

namespace ObjectPrintingHomeTask.ObjectPrinting
{
    internal static class ObjectPrinterExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            var isCollection = obj.GetType().Implements(typeof(ICollection));
            if (!isCollection)
                return ObjectPrinter.For<T>().PrintToString(obj);
            var stringToPrint = typeof(T).Name + Environment.NewLine;
            return ((ICollection) obj)
                                    .Cast<object>()
                                    .Aggregate(stringToPrint, (current, e) => current + e.PrintToString());
        }
    }
}
