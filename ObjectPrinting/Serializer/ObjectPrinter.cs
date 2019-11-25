using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        public static string PrintToString<TOwner>(TOwner obj, PrintingConfig<TOwner> config)
        {
            var interConfig = config as IPrintingConfig;
            return PrintToString(obj, 0, interConfig.SerializationRules, new HashSet<object>());
        }

        private static readonly IReadOnlyList<Type> FinalTypes = 
            new List<Type>()
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

        private const int MaximumRecursionDepth = 20;

        private static string GetPropertyPrintingStart(string indent, PropertyInfo property) =>
            indent + property.Name + " = ";

        private static string GetIndent(int level) => 
            '\n' + new string('\t', level);

        private static string PrintToString(object obj, int nestingLevel, 
            IReadOnlyList<SerializationRule> serializationRules, HashSet<object> printedObjects)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            if (FinalTypes.Contains(type))
                return obj.ToString();

            if (printedObjects.Contains(obj) || nestingLevel >= MaximumRecursionDepth)
                return "...";

            printedObjects.Add(obj);

            var buildingString = new StringBuilder();
            buildingString.Append(type.Name);

            if (type.IsArray || obj is IEnumerable)
                buildingString.Append(PrintEnumerable(obj, type, nestingLevel, serializationRules, printedObjects));
            else
                buildingString.Append(PrintProperties(obj, nestingLevel, serializationRules, printedObjects));

            return buildingString.ToString();
        }

        private static string PrintProperties(object obj, int nestingLevel,
            IReadOnlyList<SerializationRule> serializationRules, HashSet<object> printedObjects)
        {
            var properties = obj.GetType().GetProperties();
            if (properties.Length == 0)
                return "";

            var buildingString = new StringBuilder();

            foreach (var propertyInfo in properties)
            {
                var propertyResultStr = 
                    ApplyRulesToProperty(obj, propertyInfo, nestingLevel, serializationRules, printedObjects);

                if(propertyResultStr == null)
                    continue;

                buildingString.Append(propertyResultStr);
            }

            buildingString.Insert(0, GetIndent(nestingLevel) + '{');
            buildingString.Insert(buildingString.Length, GetIndent(nestingLevel) + '}');
            return buildingString.ToString();
        }

        private static string ApplyRulesToProperty(object obj, PropertyInfo propertyInfo, int nestingLevel, 
            IReadOnlyList<SerializationRule> serializationRules, HashSet<object> printedObjects)
        {
            string propertyResultStr = null;

            var indent = GetIndent(nestingLevel + 1);
            var start = GetPropertyPrintingStart(indent, propertyInfo);

            foreach (var serializationRule in serializationRules
                .Where(rule => rule.FilterHandler.Invoke(obj, propertyInfo))
                .Select(x => x.ResultHandler))
            {
                if (serializationRule == null)
                    return null;

                propertyResultStr = start + serializationRule.Invoke(obj, propertyInfo, indent, nestingLevel + 1);
            }

            
            return propertyResultStr ?? start + 
                   PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, serializationRules, printedObjects);
        }

        private static string PrintEnumerable(object obj, Type type, int nestingLevel,
            IReadOnlyList<SerializationRule> serializationRules, HashSet<object> printedObjects)
        {
            if (!(obj is IEnumerable collection))
                return "null";

            if (obj is IDictionary)
                return PrintKeyValuePairs(obj, type, nestingLevel, serializationRules, printedObjects);

            var buildingString = new StringBuilder();

            var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
            if (elementType != null)
                buildingString.Append($"<{elementType.Name}>");
            buildingString.Append(GetIndent(nestingLevel) + "{");

            var index = 0;
            foreach (var element in collection)
            {
                buildingString.Append(GetIndent(nestingLevel + 1) +
                                      $"#{index} = " + PrintToString(element, nestingLevel + 1, 
                                                                serializationRules, printedObjects));
                index++;
            }

            buildingString.Append(GetIndent(nestingLevel) + '}');
            return buildingString.ToString();
        }

        private static string PrintKeyValuePairs(object obj, Type type, int nestingLevel,
            IReadOnlyList<SerializationRule> serializationRules, HashSet<object> printedObjects)
        {
            return "\n#-#-#-#-NOT IMPLEMENTED-#-#-#-#\n";
        }
    }
}