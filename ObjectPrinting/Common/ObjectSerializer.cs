using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjectPrinting.Common
{
    internal static class ObjectSerializer
    {
        private static readonly IReadOnlyCollection<Type> finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(long),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private static readonly ObjectTreeBuilder builder = new ObjectTreeBuilder(finalTypes);

        private const string propertyFormat = "{0}{1} = {2}";

        private const string openCollectionString = "[";
        private const string closeCollectionString = "]";

        private const string openDictionaryObjectString = "{";
        private const string closeDictionaryObjectString = "}";

        private const string collectionItemSeparator = ",";

        private const char tabSymbol = '\t';

        private const string nullMark = "null";
        private const string loopReferenceMark = "loop reference";

        private static readonly Type iDictionaryType = typeof(IDictionary);
        private static readonly Type iCollectionType = typeof(ICollection);

        public static string Serialize<T>(T obj, PrintingConfigRoot configRoot)
        {
            var root = builder.BuildTree(obj, configRoot);
            return PrintObjectTreeNode(root, 0, configRoot);
        }

        private static string PrintObjectTreeNode(ObjectTreeNode node, int currentLevel, PrintingConfigRoot configRoot)
        {
            if (node.Object.Value == null)
                return nullMark;

            var fieldPropertyObject = node.Object;
            var currentType = fieldPropertyObject.Type;
            var memberInfo = fieldPropertyObject.Info;
            var value = fieldPropertyObject.Value;

            // appling property serializer
            var hasPropertySerializator = memberInfo != null && configRoot.PropertySerializers.ContainsKey(memberInfo);
            if (hasPropertySerializator)
            {
                var str = configRoot.PropertySerializers[memberInfo](value);
                return configRoot.MaxStringPropertyLengths.ContainsKey(memberInfo) ?
                       GetSubstring(str, configRoot.MaxStringPropertyLengths[memberInfo]) :
                       str;
            }

            // appling type serializer
            if (configRoot.TypeSerializers.ContainsKey(currentType))
                return configRoot.TypeSerializers[currentType](value);

            // checking for final type
            if (finalTypes.Contains(currentType))
            {
                // appling culture if exists
                var oldCulture = CultureInfo.CurrentCulture;
                if (configRoot.NumericTypeCulture.ContainsKey(currentType))
                    CultureInfo.CurrentCulture = configRoot.NumericTypeCulture[currentType];

                var str = node.Object.Value.ToString();
                CultureInfo.CurrentCulture = oldCulture;

                // appling max length for string property
                if (memberInfo != null && configRoot.MaxStringPropertyLengths.ContainsKey(memberInfo))
                    str = GetSubstring(str, configRoot.MaxStringPropertyLengths[memberInfo]);
                return str;
            }

            return PrintObjectTreeSubNodes(node, currentLevel, configRoot);
        }

        private static string PrintObjectTreeSubNodes(ObjectTreeNode node, int currentLevel, PrintingConfigRoot configRoot)
        {
            var type = node.Object.Type;

            if (iDictionaryType.IsAssignableFrom(type))
                return PrintDictionary(node, currentLevel, configRoot);

            if (iCollectionType.IsAssignableFrom(type))
                return PrintCollection(node, currentLevel, configRoot);

            return PrintFieldProperties(node, currentLevel, configRoot);
        }

        private static string PrintDictionary(ObjectTreeNode node, int currentLevel, PrintingConfigRoot configRoot)
        {
            if (node.Nodes.Count < 1)
                return openDictionaryObjectString + closeDictionaryObjectString;

            var identation = new string(tabSymbol, currentLevel + 1);
            var elementContentIdentation = new string(tabSymbol, currentLevel + 2);

            var sb = new StringBuilder();
            sb.AppendLine(openDictionaryObjectString);

            foreach (var subNode in node.Nodes)
            {
                var key = subNode.Nodes[0];
                var value = subNode.Nodes[1];

                sb.Append(identation).AppendLine(openDictionaryObjectString);
                sb.AppendFormat(propertyFormat, elementContentIdentation,
                                                "Key",
                                                PrintObjectTreeNode(key, currentLevel + 2, configRoot)).AppendLine();
                sb.AppendFormat(propertyFormat, elementContentIdentation,
                                                "Value",
                                                PrintObjectTreeNode(value, currentLevel + 2, configRoot)).AppendLine();
                sb.Append(identation).Append(closeDictionaryObjectString).AppendLine(collectionItemSeparator);
            }

            sb.Remove(sb.Length - Environment.NewLine.Length - 1, 1);
            sb.Append(new string(tabSymbol, currentLevel)).Append(closeDictionaryObjectString);

            return sb.ToString();
        }

        private static string PrintCollection(ObjectTreeNode node, int currentLevel, PrintingConfigRoot configRoot)
        {
            if (node.Nodes.Count < 1)
                return openCollectionString + closeCollectionString;

            var identation = new string(tabSymbol, currentLevel + 1);

            var sb = new StringBuilder();
            sb.AppendLine(openCollectionString);

            foreach (var subNode in node.Nodes)
            {
                string str = subNode.EndsLoop ? loopReferenceMark : PrintObjectTreeNode(subNode, currentLevel + 1, configRoot);
                sb.Append(identation).Append(str).AppendLine(collectionItemSeparator);
            }

            // Removing last ','
            sb.Remove(sb.Length - Environment.NewLine.Length - 1, 1);
            sb.Append(new string(tabSymbol, currentLevel)).Append(closeCollectionString);

            return sb.ToString();
        }

        private static string PrintFieldProperties(ObjectTreeNode node, int currentLevel, PrintingConfigRoot configRoot)
        {
            if (node.Nodes.Count < 1)
                return string.Empty;

            var identation = new string(tabSymbol, currentLevel + 1);

            var sb = new StringBuilder();
            sb.AppendLine(node.Object.Type.Name);

            foreach (var subNode in node.Nodes)
            {
                string str = subNode.EndsLoop ? loopReferenceMark : PrintObjectTreeNode(subNode, currentLevel + 1, configRoot);
                sb.AppendFormat(propertyFormat, identation, subNode.Object.Name, str).AppendLine();
            }

            return sb.Remove(sb.Length - Environment.NewLine.Length, 2).ToString();
        }

        private static string GetSubstring(string str, int maxLength)
        {
            return maxLength < str.Length ? str.Substring(0, maxLength) : str;
        }
    }
}