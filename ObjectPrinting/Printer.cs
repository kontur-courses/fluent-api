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
        private readonly Dictionary<MemberInfo, Func<object, string>>[] printingPriority;

        public Printer(Config config)
        {
            this.config = config ?? throw new ArgumentNullException();
            printingPriority = new Dictionary<MemberInfo, Func<object, string>>[]
            {
                this.config.Printing,
                this.config.PrintingWithCulture
            };
        }

        public string PrintToString(
            object obj,
            int nestingLevel = 0,
            int depthCount = 15,
            HashSet<object> printedObjects = default)
        {
            if (printedObjects == default)
                printedObjects = new HashSet<object>();
            CheckParameters(obj, nestingLevel, depthCount, printedObjects);
            var sb = new StringBuilder();
            var (value, isObjFinal) = PrintObj(obj, nestingLevel, depthCount, printedObjects);
            sb.Append(value);
            if (!isObjFinal)
                sb.Append(PrintProperties(obj, nestingLevel + 1, depthCount - 1, printedObjects));

            return sb.ToString();
        }

        protected void CheckParameters(object obj, int nestingLevel, int depthCount, HashSet<object> printedObjects)
        {
            if (obj == null || printedObjects == null)
                throw new ArgumentNullException();
            if (nestingLevel < 0 || depthCount < 0)
                throw new ArgumentOutOfRangeException();
        }

        private string PrintProperties(object obj, int nestingLevel, int depthCount, HashSet<object> printedObjects)
        {
            var newPrintedObjects = new HashSet<object>(printedObjects);
            if (obj != null && !obj.GetType().IsValueType)
                newPrintedObjects.Add(obj);
            else if (obj == null)
                return string.Empty;
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

        private bool TryGetPropertyPrinter(PropertyInfo propertyInfo, out Func<object, string> printer)
        {
            printer = null;
            if (propertyInfo == null)
                return false;
            var memberPriority = new MemberInfo[]
            {
                propertyInfo,
                propertyInfo.PropertyType
            };
            foreach (var dict in printingPriority)
                foreach (var member in memberPriority)
                    if (dict.ContainsKey(member))
                    {
                        printer = dict[member];
                        return true;
                    }
            return false;
        }

        private bool TryGetPrinter(MemberInfo memberInfo, out Func<object, string> printer)
        {
            printer = null;
            if (memberInfo == null)
                return false;
            foreach (var p in printingPriority)
                if (p.ContainsKey(memberInfo))
                {
                    printer = p[memberInfo];
                    return true;
                }
            return false;
        }

        private (string value, bool isObjFinal) PrintObj(
            object obj,
            int nestingLevel,
            int depthCount,
            HashSet<object> printedObjects,
            PropertyInfo propertyInfo = default)
        {
            var objType = obj?.GetType();
            var propertyName = propertyInfo?.Name;
            if (CheckObjAndPropertyOnExcluding(objType, propertyInfo))
                return (string.Empty, true);
            if (CheckOnExceptions(obj, printedObjects, depthCount, out var exceptionMessage))
                return (PrintValue(nestingLevel, exceptionMessage, propertyName), true);
            if (CheckOnUserPrinting(obj, propertyInfo, out var userPrinting))
                return (PrintValue(nestingLevel, userPrinting, propertyName), true);

            return GetDefaultObjectValue(obj, nestingLevel, depthCount, printedObjects, propertyName);
        }

        private (string defaultObjValue, bool isObjFinal) GetDefaultObjectValue(
            object obj,
            int nestingLevel,
            int depthCount,
            HashSet<object> printedObjects,
            string propertyName)
        {
            if (obj == null)
                return (PrintValue(nestingLevel, "null", propertyName), true);
            var objType = obj.GetType();
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

        private bool IsPropertyExcluded(PropertyInfo propertyInfo) =>
            config.Excluding.Contains(propertyInfo.PropertyType) || config.Excluding.Contains(propertyInfo);

        private bool CheckObjAndPropertyOnExcluding(Type objType, PropertyInfo propertyInfo)
        {
            return (objType != null && config.Excluding.Contains(objType)) ||
                (propertyInfo != null && IsPropertyExcluded(propertyInfo));
        }

        private bool CheckOnExceptions(
            object obj,
            HashSet<object> printedObjects,
            int depthCount,
            out string exceptionMessage)
        {
            exceptionMessage = null;
            if (obj != null && printedObjects != null && printedObjects.Contains(obj))
                exceptionMessage = "* circular reference *";
            else if (depthCount <= 0)
                exceptionMessage = "* reached maximum recursion depth *";
            return exceptionMessage != null;
        }

        private bool CheckOnUserPrinting(object obj, PropertyInfo propertyInfo, out string userPrinting)
        {
            var propPrinting = TryGetPropertyPrinter(propertyInfo, out var propertyPrinter);
            var objPrinting = TryGetPrinter(obj?.GetType(), out var printer);
            userPrinting = propPrinting ? propertyPrinter(obj) : objPrinting ? printer(obj) : null;
            return propPrinting || objPrinting;
        }

        protected string PrintValue(int nestingLevel, string propertyValue, string propertyName = default) =>
            propertyName == default ?
                $"{new string('\t', nestingLevel)}{propertyValue}{Environment.NewLine}" :
                $"{new string('\t', nestingLevel)}{propertyName} = {propertyValue}{Environment.NewLine}";
    }
}