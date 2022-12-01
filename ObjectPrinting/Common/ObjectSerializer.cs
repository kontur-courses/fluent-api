using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Common
{
    internal static class ObjectSerializer
    {
        private static readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private const string propertyFormat = "{0}{1} = {2}";

        public static string Serialize<T>(T obj, PrintingConfigRoot configRoot)
        {
            var root = BuildObjectTree(obj, configRoot);
            var str = PrintObjectTreeNode(root, 0, configRoot);

            return str;
        }

        private static ObjectTreeNode BuildObjectTree(object obj, PrintingConfigRoot configRoot)
        {
            var root = new ObjectTreeNode()
            {
                Parent = null,
                Value = obj
            };

            var nodeStack = new Stack<ObjectTreeNode>();
            nodeStack.Push(root);

            while (nodeStack.Count > 0)
            {
                var currentNode = nodeStack.Pop();
                var currentType = currentNode.Value.GetType();

                if (configRoot.ExcludedTypes.Contains(currentType))
                    continue;

                if (finalTypes.Contains(currentType))
                    continue;

                var properties = currentType.GetProperties()
                                            .Where(prop => !configRoot.ExcludedProperties.Contains(prop) &&
                                                           !configRoot.ExcludedTypes.Contains(prop.PropertyType))
                                            .ToArray();

                foreach (var property in properties)
                {
                    var propertyValue = property.GetValue(currentNode.Value);

                    var propertyEndsLoop = false;
                    if (!finalTypes.Contains(property.PropertyType))
                    {
                        var parentNode = currentNode;
                        do
                        {
                            if (parentNode.Value.GetType().IsValueType || parentNode.Value != propertyValue)
                            {
                                parentNode = parentNode.Parent;
                                continue;
                            }

                            propertyEndsLoop = true;
                            break;
                        } while (parentNode != null);
                    }

                    var subNode = new ObjectTreeNode()
                    {
                        Value = propertyValue,
                        EndsLoop = propertyEndsLoop,
                        Parent = currentNode,
                        PropertyInfo = property
                    };
                    currentNode.Nodes.Add(subNode);
                }

                foreach (var subNode in currentNode.Nodes.Reverse<ObjectTreeNode>())
                    nodeStack.Push(subNode);
            }

            return root;
        }

        private static string PrintObjectTreeNode(ObjectTreeNode node, int currentLevel, PrintingConfigRoot configRoot)
        {
            if (node.Value == null)
                return "null";

            var currentType = currentLevel == 0 ? node.Value.GetType() : node.PropertyInfo.PropertyType;

            // appling property serializer
            if (node.PropertyInfo != null && configRoot.PropertySerializers.ContainsKey(node.PropertyInfo))
            {
                string str;
                if(configRoot.MaxStringPropertyLengths.ContainsKey(node.PropertyInfo))
                {
                    int maxLength = configRoot.MaxStringPropertyLengths[node.PropertyInfo];
                    str = (string)node.Value;
                    str = maxLength < str.Length ? str.Substring(maxLength) : str;
                    return configRoot.PropertySerializers[node.PropertyInfo](str);
                }

                return configRoot.PropertySerializers[node.PropertyInfo](node.Value);
            }

            // checking for final type
            if (finalTypes.Contains(currentType))
            {
                // appling culture if exists
                var oldCulture = CultureInfo.CurrentCulture;
                if (configRoot.NumericTypeCulture.ContainsKey(currentType))
                    CultureInfo.CurrentCulture = configRoot.NumericTypeCulture[currentType];

                var str = node.Value.ToString();
                CultureInfo.CurrentCulture = oldCulture;

                // appling max length for string property
                if (node.PropertyInfo != null &&
                    configRoot.MaxStringPropertyLengths.ContainsKey(node.PropertyInfo) &&
                    configRoot.MaxStringPropertyLengths[node.PropertyInfo] < str.Length)
                    str = str.Substring(configRoot.MaxStringPropertyLengths[node.PropertyInfo]);
                return str;
            }

            if (node.Nodes.Count < 1)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(currentType.Name);

            var identation = new string('\t', currentLevel + 1);
            foreach (var subNode in node.Nodes)
            {
                if (subNode.EndsLoop)
                {
                    sb.AppendFormat(propertyFormat, identation, subNode.PropertyInfo.Name, "cycle reference").AppendLine();
                    continue;
                }

                sb.AppendFormat(propertyFormat, identation,
                                                subNode.PropertyInfo.Name,
                                                PrintObjectTreeNode(subNode, currentLevel + 1, configRoot)).AppendLine();
            }

            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }
}