using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting.Common
{
    internal class ObjectTreeBuilder
    {
        private readonly IReadOnlyCollection<Type> finalTypes;

        public ObjectTreeBuilder(IReadOnlyCollection<Type> finalTypes)
        {
            this.finalTypes = finalTypes;
        }

        public ObjectTreeNode BuildTree(object obj, PrintingConfigRoot configRoot)
        {
            var root = new ObjectTreeNode()
            {
                Parent = null,
                Object = new ValueObject(obj)
            };

            var nodeStack = new Stack<ObjectTreeNode>();
            nodeStack.Push(root);

            ObjectTreeNode currentNode;
            while (nodeStack.Count > 0)
            {
                currentNode = nodeStack.Pop();
                var currentType = currentNode.Object.Type;

                if (configRoot.ExcludedTypes.Contains(currentType))
                    continue;

                if (finalTypes.Contains(currentType))
                    continue;

                var subNodes = GetSubNodes(currentNode, configRoot);
                currentNode.Nodes.AddRange(subNodes);

                foreach (var subNode in currentNode.Nodes.Where(node => !node.EndsLoop).Reverse())
                    nodeStack.Push(subNode);
            }

            return root;
        }

        private IEnumerable<ObjectTreeNode> GetSubNodes(ObjectTreeNode node, PrintingConfigRoot configRoot)
        {
            var nodeType = node.Object.Type;

            if (node.Object.Value is null)
                return Enumerable.Empty<ObjectTreeNode>();

            if (typeof(ICollection).IsAssignableFrom(nodeType))
                return GetCollectionNodes(node);
            return GetFieldPropertyNodes(node, configRoot);
        }

        private IEnumerable<ObjectTreeNode> GetCollectionNodes(ObjectTreeNode node)
        {
            foreach (var item in (ICollection)node.Object.Value)
            {
                var obj = new ValueObject(item);
                var endsLoop = !finalTypes.Contains(obj.Type) &&
                               IsValueEndsLoop(node, obj.Value);

                yield return new ObjectTreeNode()
                {
                    Object = obj,
                    EndsLoop = endsLoop,
                    Parent = node
                };
            }
        }

        private IEnumerable<ObjectTreeNode> GetFieldPropertyNodes(ObjectTreeNode node, PrintingConfigRoot configRoot)
        {
            var nodeType = node.Object.Type;

            var fieldsAndProperties = GetFieldsAndProperties(nodeType).Where(member => !IsMemberExcluded(member, configRoot));
            foreach (var fieldPropertyObject in fieldsAndProperties)
            {
                fieldPropertyObject.SetValue(node.Object.Value);
                var endsLoop = !finalTypes.Contains(fieldPropertyObject.Type) &&
                               IsValueEndsLoop(node, fieldPropertyObject.Value);

                yield return new ObjectTreeNode()
                {
                    Object = fieldPropertyObject,
                    EndsLoop = endsLoop,
                    Parent = node
                };
            }
        }

        private static IEnumerable<FieldPropertyObject> GetFieldsAndProperties(Type type)
        {
            var properties = type.GetProperties().Select(prop => (FieldPropertyObject)new PropertyObject(prop));
            var fields = type.GetFields().Select(field => (FieldPropertyObject)new FieldObject(field));
            return properties.Concat(fields);
        }

        private static bool IsMemberExcluded(FieldPropertyObject obj, PrintingConfigRoot configRoot)
        {
            return configRoot.ExcludedProperties.Contains(obj.Info) &&
                   configRoot.ExcludedTypes.Contains(obj.Type);
        }

        private static bool IsValueEndsLoop(ObjectTreeNode currentNode, object value)
        {
            var endsLoop = false;
            var parentNode = currentNode;
            while (parentNode != null)
            {
                endsLoop = !parentNode.Object.Type.IsValueType && parentNode.Object.Value == value;
                if (endsLoop)
                    break;

                parentNode = parentNode.Parent;
            }

            return endsLoop;
        }
    }
}