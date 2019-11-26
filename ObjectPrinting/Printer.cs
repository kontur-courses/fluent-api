using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    class Printer
    {
        private readonly Config config;

        public Printer(Config config) => this.config = config;

        public string PrintToString(object obj)
        {
            var nestingLevel = 0;
            var depthCount = 15;
            var printedObjects = new HashSet<object>();

            return PrintToString(obj, nestingLevel, depthCount, printedObjects);
        }

        public string PrintToString(object obj, int nestingLevel, int depthCount, HashSet<object> printedObjects)
        {
            var sb = new StringBuilder();
            var (value, isObjFinal) = PrintObj(obj, nestingLevel, depthCount, printedObjects);
            sb.Append(value);
            if (!isObjFinal)
                sb.Append(PrintProperties(obj, nestingLevel + 1, depthCount - 1, printedObjects));

            return sb.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel, int depthCount, HashSet<object> printedObjects)
        {
            var newPrintedObjects = new HashSet<object>(printedObjects);
            if (obj != null && !obj.GetType().IsValueType)
                newPrintedObjects.Add(obj);
            var sb = new StringBuilder();
            foreach (var propInfo in obj
                .GetType()
                .GetProperties()
                .OrderBy(p => p.Name))
            {
                var propObj = propInfo.GetValue(obj);
                var (value, isObjFinal) = PrintObj(propObj, nestingLevel, depthCount, printedObjects, propInfo);
                sb.Append(value);
                if (!isObjFinal)
                    sb.Append(PrintProperties(propObj, nestingLevel + 1, depthCount - 1, newPrintedObjects));
            }
            return sb.ToString();
        }

        private bool IsPropertyExcluded(PropertyInfo propertyInfo) =>
            config.Excluding.Contains(propertyInfo.PropertyType) || config.Excluding.Contains(propertyInfo);

        private bool TryGetPrinter(PropertyInfo propertyInfo, out Func<object, string> printer)
        {
            if (propertyInfo == null)
            {
                printer = null;
                return false;
            }
            var printingPriority = new[]
            {
                config.Printing,
                config.PrintingWithCulture
            };
            var memberPriority = new MemberInfo[]
            {
                propertyInfo,
                propertyInfo.PropertyType
            };
            Func<object, string> printToString = null;
            var found = printingPriority.Any(
                dict => memberPriority.Any(
                    member =>
                    {
                        if (dict.ContainsKey(member))
                        {
                            printToString = propObj => dict[member](propObj);
                            return true;
                        }
                        return false;
                    }));
            printer = found ? printToString : null;
            return found;
        }

        private (string value, bool isObjFinal) PrintObj(
            object obj,
            int nestingLevel,
            int depthCount,
            HashSet<object> printedObjects,
            PropertyInfo propertyInfo = default)
        {
            var propertyName = propertyInfo?.Name;
            if (propertyInfo != null && IsPropertyExcluded(propertyInfo))
                return (string.Empty, true);
            if (obj != null && printedObjects.Contains(obj))
                return (PrintValue(nestingLevel, "* circular reference *", propertyName), true);
            if (depthCount <= 0)
                return (PrintValue(nestingLevel, "* reached maximum recursion depth *", propertyName), true);
            var objType = obj?.GetType();
            if (objType == null)
                return (PrintValue(nestingLevel, "null", propertyName), true);
            if (config.Excluding.Contains(objType))
                return (string.Empty, true);
            if (TryGetPrinter(propertyInfo, out var printer))
                return (PrintValue(nestingLevel, printer(obj), propertyName), true);
            if (config.FinalTypes.Contains(objType))
                return (PrintValue(nestingLevel, obj.ToString(), propertyName), true);
            if (objType.GetInterface(typeof(IEnumerable).Name) != null)
                return 
                    (new EnumerablePrinter(config).PrintToString(
                        (IEnumerable)obj,
                        nestingLevel,
                        depthCount,
                        printedObjects,
                        propertyName),
                    true);
            return (PrintValue(nestingLevel, objType.Name, propertyName), false);
        }

        protected string PrintValue(int nestingLevel, string propertyValue, string propertyName = default) =>
            propertyName == default ?
                $"{new string('\t', nestingLevel)}{propertyValue}{Environment.NewLine}" :
                $"{new string('\t', nestingLevel)}{propertyName} = {propertyValue}{Environment.NewLine}";
    }
}