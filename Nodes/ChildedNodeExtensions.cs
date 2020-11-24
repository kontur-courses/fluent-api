using System;
using System.Collections.Generic;
using System.Linq;

namespace Nodes
{
    public static class ChildedNodeExtensions
    {
        public static bool TryGetChild<TPayload>(this IChildedNode<TPayload> node,
            string childName, out IParentedNode<TPayload> child) => node.ChildNodes.TryGetValue(childName, out child);

        public static void AddChild<TPayload>(this IChildedNode<TPayload> node, IParentedNode<TPayload> child)
        {
            node.ChildNodes.Add(child.Name, child);
            child.Parent = node;
        }

        public static IParentedNode<TPayload> GetChild<TPayload>(this IChildedNode<TPayload> node, string childName) =>
            node.TryGetChild(childName, out var child)
                ? child
                : throw new ArgumentException($"Node {node.Name} does not contain child with name {childName}");

        public static IEnumerable<IParentedNode<TPayload>> GetChildNodes<TPayload>(this IChildedNode<TPayload> node) =>
            node.ChildNodes.Select(d => d.Value);
    }
}