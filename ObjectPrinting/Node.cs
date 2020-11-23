using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting
{
    public class Node
    {
        public readonly string Name;
        private readonly IDictionary<string, Node> childNodes = new Dictionary<string, Node>();

        /// <summary>
        /// Node where current node is containted. May be null when current node is root.
        /// </summary>
        private Node? Parent { get; set; }

        public Node(string name)
        {
            Name = name;
        }

        public string FullPath => Parent?.Name + $".{Name}";

        public void AddChild(Node child)
        {
            childNodes.Add(child.Name, child);
            child.Parent = this;
        }

        public bool TryGetChild(string childName, out Node child) =>
            childNodes.TryGetValue(childName, out child);

        public Node GetChild(string childName) => TryGetChild(childName, out var child)
            ? child
            : throw new ArgumentException($"Node {Name} does not contain child with name {childName}");

        public IEnumerable<Node> ChildNodes => childNodes.Select(d => d.Value);

        public static Node Root(string name) => new Node(name);

        public static Node Child(string name, Node parent)
        {
            var child = new Node(name);
            parent.AddChild(child);
            return child;
        }
    }
}