using System;
using System.Collections;
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

        

        public string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(ResolveTypeName(type));

            if (Implements(type, typeof(IList)))
                return sb.Append(PrintIList(obj, type, nestingLevel)).ToString();

            if (alreadyHandledObjects.Contains(obj))
                return "(cycle)" + NewLine;
            alreadyHandledObjects.Add(obj);

            return sb.Append(PrintComplexType(obj, type, nestingLevel)).ToString();
        }

        private string ResolveTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var sb = new StringBuilder();
            sb.Append(type.Name.Substring(0, type.Name.Length - 2) + "<");

            var genericArguments = type.GetGenericArguments();

            for (var index = 0; index < genericArguments.Length - 1; index++)
                sb.Append(ResolveTypeName(genericArguments[index]) + ", ");
            sb.Append(ResolveTypeName(genericArguments[genericArguments.Length - 1]) + ">");

            return sb.ToString();
        }

        private bool Implements(Type type, Type interfaceName)
        {
            return type.GetInterfaces().Any(t => t == interfaceName);
        }

        private string PrintIList(object obj, Type type, int nestingLevel)
        {
            var sb = new StringBuilder();
            var bracesIndentation = new string('\t', nestingLevel);
            sb.Append($"{bracesIndentation}[{NewLine}");

            foreach (var element in obj as IEnumerable)
                sb.Append(bracesIndentation + "\t" + PrintToString(element) + NewLine);

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
    }
}
