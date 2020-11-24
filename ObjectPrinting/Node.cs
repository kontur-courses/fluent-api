using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting
{
    public class Node<TPayload>
    {
        /// <summary>
        /// Node where current node is containted. May be null when current node is root.
        /// </summary>
        private Node<TPayload>? parent;
        private readonly IDictionary<string, Node<TPayload>> childNodes = new Dictionary<string, Node<TPayload>>();

        public Node(string name)
        {
            Name = name;
        }

        public Node(string name, TPayload payload) : this(name)
        {
            Payload = payload;
        }

        public TPayload Payload { get; set; }
        public string Name { get; }
        public string FullPath => parent?.Name + $".{Name}";

        public void AddChild(Node<TPayload> child)
        {
            childNodes.Add(child.Name, child);
            child.parent = this;
        }

        public bool TryGetChild(string childName, out Node<TPayload> child) =>
            childNodes.TryGetValue(childName, out child);

        public Node<TPayload> GetChild(string childName) => TryGetChild(childName, out var child)
            ? child
            : throw new ArgumentException($"Node {Name} does not contain child with name {childName}");

        public IEnumerable<Node<TPayload>> ChildNodes => childNodes.Select(d => d.Value);

        public static Node<TPayload> Root(string name) => new Node<TPayload>(name);

        public static Node<TPayload> Child(string name, Node<TPayload> parent)
        {
            var child = new Node<TPayload>(name);
            parent.AddChild(child);
            return child;
        }
    }
}