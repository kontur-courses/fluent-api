using System;
using System.Collections;
using System.Text;

namespace ObjectPrinting
{
    public static class SpecificTypesSerializer
    {
        public static string PrintArray(IEnumerable enumerable, int nestingLevel,
            Func<object, int, bool, string> printToStr)
        {
            return PrintEnumerable(enumerable, nestingLevel,
                el => printToStr(el, nestingLevel + 1, true));
        }

        public static string PrintDictionary(IDictionary dictionary, int nestingLevel,
            Func<object, int, bool, string> printToStr)
        {
            return PrintEnumerable(dictionary.Keys, nestingLevel,
                kvp => printToStr(kvp, nestingLevel + 2, false) +
                       " : " + printToStr(dictionary[kvp], nestingLevel + 2, true));
        }

        private static string PrintEnumerable(IEnumerable enumerable, int nestingLevel,
            Func<object, string> serializeElement)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            var index = 0;
            foreach (var el in enumerable)
                sb.Append($"{identation}[{index++}] = {serializeElement(el)}");

            return sb.ToString();
        }
    }
}