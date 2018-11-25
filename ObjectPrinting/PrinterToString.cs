using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class PrinterToString
    {
        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private static readonly string NewLine = Environment.NewLine;

        private readonly HashSet<object> alreadyHandledObjects;
        private readonly ConfigsContainer configsContainer;

        public PrinterToString(ConfigsContainer configsContainer)
        {
            alreadyHandledObjects = new HashSet<object>();
            this.configsContainer = configsContainer;
        }

        public string PrintToString(object obj)
        {
            return PrintToString(obj, 0);
        }

        private bool Implements(Type type, string interfaceName)
        {
            return type.GetInterfaces().Any(t => t.Name == interfaceName);
        }

        private string PrintList(object obj, Type type, int nestingLevel)
        {
            var sb = new StringBuilder();
            var bracesIndentation = new string('\t', nestingLevel);
            sb.Append($"{bracesIndentation}[{NewLine}");
            var count = (int)type.GetProperty("Count").GetValue(obj);
            for (var index = 0; index < count; index++)
            {
                var item = type.GetProperty("Item").GetValue(obj, new object[] { index });
                sb.Append(bracesIndentation + "\t" + PrintToString(item) + NewLine);
            }

            sb.Append($"{bracesIndentation}]{NewLine}");
            return sb.ToString();
        }

        private string PrintArray(object obj, Type type, int nestingLevel)
        {
            var sb = new StringBuilder();
            var bracesIndentation = new string('\t', nestingLevel);
            sb.Append($"{bracesIndentation}[{NewLine}");
            var count = (int)type.GetProperty("Length").GetValue(obj);
            for (var index = 0; index < count; index++)
            {
                var methodInfo = type.GetMethod("GetValue", new [] { typeof(Int32) });
                var item = methodInfo.Invoke(obj, new object[] { index });
                sb.Append(bracesIndentation + "\t" + PrintToString(item) + NewLine);
            }

            sb.Append($"{bracesIndentation}]{NewLine}");
            return sb.ToString();
        }

        private string PrintComplexType(object obj, Type type, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var propertyInfo in type.GetProperties())
            {
                if (configsContainer.TypesToExclude.Contains(propertyInfo.PropertyType))
                    continue;
                if (configsContainer.PropertiesToExclude.Contains(propertyInfo.Name))
                    continue;

                sb.Append(indentation + propertyInfo.Name + " = ");
                if (configsContainer.PrintersForProperties.ContainsKey(propertyInfo.Name))
                    sb.Append(configsContainer.PrintersForProperties[propertyInfo.Name](propertyInfo.GetValue(obj))
                              + NewLine);
                else if (configsContainer.PrintersForTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.Append(configsContainer.PrintersForTypes[propertyInfo.PropertyType](propertyInfo.GetValue(obj))
                              + NewLine);
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        public string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (Implements(type, "List`1"))
                return sb.Append(PrintList(obj, type, nestingLevel)).ToString();
            if (type.IsArray)
                return sb.Append(PrintArray(obj, type, nestingLevel)).ToString();

            if (alreadyHandledObjects.Contains(obj))
                return "(cycle)" + NewLine;
            alreadyHandledObjects.Add(obj);

            return sb.Append(PrintComplexType(obj, type, nestingLevel)).ToString();
        }
    }
}
