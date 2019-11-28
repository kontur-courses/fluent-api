using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectPrinter
    {
        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan)
        };

        private readonly IPrintingConfig config;

        internal ObjectPrinter(IPrintingConfig config)
        {
            this.config = config;
        }

        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        internal string PrintToString(object obj)
        {
            return PrintToString(obj, new HashSet<object>());
        }

        private string PrintToString(object obj, ISet<object> externalObjects, int nestingLevel = 0)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (externalObjects.Contains(obj))
                return type.Name + " ↰" + Environment.NewLine;

            if (config.TypePrinters.TryGetValue(type, out var print))
                return print.DynamicInvoke(obj) + Environment.NewLine;

            if (!FinalTypes.Contains(type))
                return PrintObject(obj, nestingLevel, externalObjects);

            if (config.CultureLookup.TryGetValue(type, out var cultureInfo))
                return Convert.ToString(obj, cultureInfo) + Environment.NewLine;

            return obj + Environment.NewLine;
        }

        private string PrintObject(object obj, int nestingLevel, ISet<object> externalObjects)
        {
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            externalObjects.Add(obj);
            PrintElements(obj, externalObjects, nestingLevel, sb);
            externalObjects.Remove(obj);
            return sb.ToString();
        }

        private void PrintElements(object obj, ISet<object> externalObjects, int nestingLevel, StringBuilder sb)
        {
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var elementInfo in obj.GetElements().Where(IsPropertyIncluded))
            {
                sb.Append(indentation).Append(elementInfo.Name).Append(" = ");
                var value = elementInfo.GetValue(obj);
                if (value != null && elementInfo.MemberInfo != null &&
                    config.PropertyPrinters.TryGetValue(elementInfo.MemberInfo, out var printingFunction))
                    sb.Append(printingFunction.DynamicInvoke(value));
                else
                    sb.Append(PrintToString(value, externalObjects, nestingLevel + 1));
            }
        }

        private bool IsPropertyIncluded(ElementInfo element)
        {
            return !config.ExcludedTypes.Contains(element.Type) &&
                   !config.ExcludedProperties.Contains(element.MemberInfo);
        }
    }
}