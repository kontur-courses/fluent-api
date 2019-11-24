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

        public string PrintToString(object obj, int nestingLevel, int depthCount, HashSet<object> usedObjects)
        {
            if (usedObjects.Contains(obj))
                return "* circular reference *" + Environment.NewLine;
            if (obj != null && !obj.GetType().IsValueType)
                usedObjects.Add(obj);
            if (depthCount <= 0)
                return "* reached maximum recursion depth *" + Environment.NewLine;

            if (TryGetSpecialPrinter(obj?.GetType(), out var printer))
                return printer(obj, nestingLevel, depthCount, usedObjects);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in
                type
                .GetProperties()
                .Where(p => !IsPropertyExcluded(p))
                .OrderBy(p => p.Name))
                sb.Append(identation + propertyInfo.Name + " = " +
                          GetPrinter(propertyInfo)
                          .Invoke(propertyInfo.GetValue(obj), nestingLevel + 1, depthCount - 1, new HashSet<object>(usedObjects)));
            return sb.ToString();
        }

        private bool TryGetSpecialPrinter(
            Type objType,
            out Func<object, int, int, HashSet<object>, string> printer)
        {
            printer = null;
            var found = true;
            if (objType == null)
                printer = (obj, nestingLvl, depthCount, usedObjs) => "null" + Environment.NewLine;
            else if (config.Excluding.Contains(objType))
                printer = (obj, nestingLvl, depthCount, usedObjs) => string.Empty;
            else if (config.Printing.ContainsKey(objType))
                printer = (obj, nestingLvl, depthCount, usedObjs) => config.Printing[objType].Invoke(obj);
            else if (config.FinalTypes.Contains(objType))
                printer = (obj, nestingLvl, depthCount, usedObjs) => obj + Environment.NewLine;
            else if (objType.GetInterface(typeof(IEnumerable).Name) != null)
                printer = new EnumerablePrinter(config).PrintToString;
            else
                found = false;

            return found;
        }

        private bool IsPropertyExcluded(PropertyInfo propertyInfo) =>
            config.Excluding.Contains(propertyInfo.PropertyType) || config.Excluding.Contains(propertyInfo);

        private Func<object, int, int, HashSet<object>, string> GetPrinter(PropertyInfo propertyInfo)
        {
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
            Func<object, int, int, HashSet<object>, string> printToString = PrintToString;
            printingPriority.Any(
                dict => memberPriority.Any(
                    member =>
                    {
                        if (dict.ContainsKey(member))
                        {
                            printToString = (propObj, nestingLvl, depthCount, usedObjs) => dict[member](propObj) + Environment.NewLine;
                            return true;
                        }
                        return false;
                    }));
            return printToString;
        }
    }
}