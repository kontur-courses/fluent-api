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
            InspectCircularReferences(obj);
            LabelCircularReferences();
            printedCircularlyReferencedObjects = new HashSet<object>();
            return PrintToString(obj, 0);
        }

        public string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + NewLine;

            if (obj.GetType().IsFinal())
                return obj + NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(PrintTypeName(type));

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
            if (Implements(type, typeof(IDictionary)))
                return sb.Append(PrintIDictionary(obj, nestingLevel)).ToString();
            if (Implements(type, typeof(IEnumerable)))
                return sb.Append(PrintIEnumerable(obj, '[', ']', nestingLevel)).ToString();

            return sb.Append(PrintComplexType(obj, type, nestingLevel)).ToString();
        }

        private void InspectCircularReferences(object obj)
        {
            var typesToSkip = new HashSet<Type>(
                configsContainer.TypesToExclude.Union(configsContainer.PrintersForTypes.Keys));
            var propertiesToSkip = new HashSet<PropertyInfo>(
                configsContainer.PropertiesToExclude.Union(configsContainer.PrintersForProperties.Keys));

            var inspector = new CircularRefsInspector(obj, typesToSkip, propertiesToSkip);
            circularlyReferencedObjects = inspector.GetCircularlyReferencedObjects();
        }

        private void LabelCircularReferences()
        {
            circularlyReferencedObjectsLabels = new Dictionary<object, int>();
            var label = 0;
            foreach (var referencedObject in circularlyReferencedObjects)
            {
                circularlyReferencedObjectsLabels[referencedObject] = label;
                label++;
            }
        }

        private string PrintTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var sb = new StringBuilder();
            sb.Append(type.Name.Substring(0, type.Name.Length - 2) + "<");

            var genericArguments = type.GetGenericArguments();

            for (var index = 0; index < genericArguments.Length - 1; index++)
                sb.Append(PrintTypeName(genericArguments[index]) + ", ");
            sb.Append(PrintTypeName(genericArguments[genericArguments.Length - 1]) + ">");

            return sb.ToString();
        }

        private bool Implements(Type type, Type interfaceName)
        {
            return type.GetInterfaces().Any(t => t == interfaceName);
        }

        private string PrintIList(object obj, int nestingLevel)
        {
            return PrintIEnumerable(obj, '[', ']', nestingLevel);
        }

        private string PrintIDictionary(object obj, int nestingLevel)
        {
            return PrintIEnumerable(obj, '{', '}', nestingLevel);
        }

        private string PrintIEnumerable(object obj, char openBrace, char closeBrace, int nestingLevel)
        {
            var sb = new StringBuilder();
            var bracesIndentation = new string('\t', nestingLevel);
            sb.AppendLine($"{bracesIndentation}{openBrace}");

            foreach (var element in obj as IEnumerable)
                sb.AppendLine(bracesIndentation + "\t" + PrintToString(element, nestingLevel + 1));

            sb.Append($"{bracesIndentation}{closeBrace}");
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
                    sb.AppendLine(configsContainer.PrintersForProperties[propertyInfo](propertyInfo.GetValue(obj)));
                else if (configsContainer.PrintersForTypes.ContainsKey(propertyInfo.PropertyType))
                    sb.AppendLine(configsContainer.PrintersForTypes[propertyInfo.PropertyType](propertyInfo.GetValue(obj)));
                else
                    sb.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}
