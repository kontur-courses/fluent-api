using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private HashSet<object> circularlyReferencedObjects;
        private Dictionary<object, int> circularlyReferencedObjectsLabels;
        private HashSet<object> printedCircularlyReferencedObjects;
        private readonly ConfigsContainer configsContainer;

        public PrinterToString(ConfigsContainer configsContainer)
        {
            circularlyReferencedObjects = new HashSet<object>();
            this.configsContainer = configsContainer;
        }

        public string PrintToString(object obj)
        {
            var typesToSkip = new HashSet<Type>(
                configsContainer.TypesToExclude.Union(configsContainer.PrintersForTypes.Keys));
            var propertiesToSkip = new HashSet<PropertyInfo>(
                configsContainer.PropertiesToExclude.Union(configsContainer.PrintersForProperties.Keys));

            var inspector = new CircularRefsInspector(obj, typesToSkip, propertiesToSkip);
            circularlyReferencedObjects = inspector.GetCircularlyReferencedObjects();
            circularlyReferencedObjectsLabels = new Dictionary<object, int>();
            var label = 0;
            foreach (var referencedObject in circularlyReferencedObjects)
            {
                circularlyReferencedObjectsLabels[referencedObject] = label;
                label++;
            }
            
            printedCircularlyReferencedObjects = new HashSet<object>();
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
            sb.Append(ResolveTypeName(type));

            if (circularlyReferencedObjects.Contains(obj))
            {
                if (printedCircularlyReferencedObjects.Contains(obj))
                    return sb.AppendLine(
                        $" (cycle reference to label {circularlyReferencedObjectsLabels[obj]})")
                        .ToString();

                printedCircularlyReferencedObjects.Add(obj);
                sb.Append($" (label {circularlyReferencedObjectsLabels[obj]})");
            }
            sb.AppendLine();

            if (Implements(type, typeof(IList)))
                return sb.Append(PrintIList(obj, nestingLevel)).ToString();

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

        private string PrintIList(object obj, int nestingLevel)
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
                if (configsContainer.PropertiesToExclude.Contains(propertyInfo))
                    continue;

                sb.Append(indentation + propertyInfo.Name + " = ");
                if (configsContainer.PrintersForProperties.ContainsKey(propertyInfo))
                    sb.Append(configsContainer.PrintersForProperties[propertyInfo](propertyInfo.GetValue(obj))
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
