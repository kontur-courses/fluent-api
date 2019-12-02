using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Formatting;
using ObjectPrinting.PrintingConfigs;
using ObjectPrinting.Serializer;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static PrintingConfig<T> For<T>()
        {
            return new PrintingConfig<T>();
        }

        public static string PrintToString<TOwner>(object obj, PrintingConfig<TOwner> configuration)
        {
            var interConfig = configuration as IPrintingConfig;

            var serializationConfig = new SerializationConfiguration(interConfig.SerializationRules,
                interConfig.InstalledFormatting ?? FormatConfiguration.Default);
            
            return PrintToString(obj, 0, serializationConfig);
        }

        private static string PrintToString(object obj, int nestingLevel, SerializationConfiguration config)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            if (config.Formatting.FinalTypes.Contains(type))
                return obj.ToString();

            if (config.CanPrintIfRecursion(obj, nestingLevel)) //TODO: Rename
                return "...";

            config.PrintedObjects.Add(obj);

            var buildingString = new StringBuilder();
            buildingString.Append(type.Name);

            if (type.IsArray || obj is IEnumerable)
                buildingString.Append(PrintEnumerable(obj, type, nestingLevel, config));
            else
                buildingString.Append(PrintProperties(obj, nestingLevel, config));

            return buildingString.ToString();
        }

        private static string PrintProperties(object obj, int nestingLevel, SerializationConfiguration config)
        {
            var properties = obj.GetType().GetProperties();
            if (properties.Length == 0)
                return "";

            var buildingString = new StringBuilder();

            foreach (var propertyInfo in properties)
            {
                if(!TrySerializeProperty(obj, propertyInfo, nestingLevel, config, out var propertyResultStr))
                    continue;

                buildingString.Append(propertyResultStr);
            }

            buildingString.Insert(0, config.Formatting.GetBorder(nestingLevel, true));
            buildingString.Insert(buildingString.Length, config.Formatting.GetBorder(nestingLevel, false));
            
            return buildingString.ToString();
        }

        private static bool TrySerializeProperty(object obj, PropertyInfo propertyInfo, int nestingLevel,
            SerializationConfiguration config, out string resultStr)
        {
            resultStr = null;

            var start = config.Formatting.GetPropertyPrintingStart(nestingLevel + 1, propertyInfo);

            foreach (var serializationRule in config.SerializationRules
                .Where(rule => rule.FilterHandler.Invoke(obj, propertyInfo))
                .Select(x => x.FormattingHandler))
            {
                if (serializationRule == null)
                    return false;

                resultStr = start + serializationRule.Invoke(obj, propertyInfo);
            }
            
            resultStr = resultStr ?? start + 
                   PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, config);
            return true;
        }

        private static string PrintEnumerable(object obj, Type type, int nestingLevel,
            SerializationConfiguration config)
        {
            if (!(obj is IEnumerable collection))
                return "null";

            if (obj is IDictionary)
                return PrintKeyValuePairs(obj, type, nestingLevel, config);

            var buildingString = new StringBuilder();

            var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
            if (elementType != null)
                buildingString.Append(config.Formatting.GetGenericVisualisation(elementType));
            
            
            buildingString.Append(config.Formatting.GetBorder(nestingLevel, true));
            
            var index = 0;
            foreach (var element in collection)
            {
                buildingString.Append(config.Formatting.GetEnumerableIndent(nestingLevel + 1, index) + 
                                      PrintToString(element, nestingLevel + 1, config));
                index++;
            }

            buildingString.Append(config.Formatting.GetBorder(nestingLevel, false));
            return buildingString.ToString();
        }

        private static string PrintKeyValuePairs(object obj, Type type, int nestingLevel,
            SerializationConfiguration config)
        {
            var buildingString = new StringBuilder();

            buildingString.Append(config.Formatting.GetGenericVisualisation(type.GetGenericArguments()));
            buildingString.Append(config.Formatting.GetBorder(nestingLevel, true));

            var dict = obj as IDictionary;
            foreach (var key in dict.Keys)
            {
                buildingString.Append(config.Formatting.GetDictionaryElementVisualisation(nestingLevel + 1,
                    key.ToString(), PrintToString(dict[key], nestingLevel + 1, config)));
            }

            buildingString.Append(config.Formatting.GetBorder(nestingLevel, false));
            return buildingString.ToString();
        }
    }
}