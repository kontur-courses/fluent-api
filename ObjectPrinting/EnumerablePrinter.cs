﻿using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    class EnumerablePrinter : Printer
    {
        public EnumerablePrinter(Config config) : base(config)
        {}

        public string PrintToString(
            IEnumerable enumerable,
            int nestingLevel,
            int depthCount,
            HashSet<object> printedObjects,
            string propertyName)
        {
            var newPrintedObjects = new HashSet<object>(printedObjects) { enumerable };
            var sb = new StringBuilder();
            sb.Append(PrintValue(nestingLevel, enumerable.GetType().Name, propertyName));
            foreach (var e in enumerable)
                sb.Append(PrintToString(e, nestingLevel + 1, depthCount - 1, newPrintedObjects));
            return sb.ToString();
        }
    }
}