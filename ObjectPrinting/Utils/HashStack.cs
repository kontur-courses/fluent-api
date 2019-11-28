using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Utils
{
    public class HashStack<T> : IEnumerable<T>
    {
        private class HashStackNode<T>
        {
            public HashStackNode<T> Previous;
            public HashStackNode<T> Next;
            public T Value;

            public HashStackNode(T value)
            {
                Value = value;
            }
        }

        private HashStackNode<T> first;
        private readonly Dictionary<T, HashStackNode<T>> nodes;

        public HashStack()
        {
            nodes = new Dictionary<T, HashStackNode<T>>();
        }

        public void Push(T value)
        {
            if (nodes.ContainsKey(value))
                throw new ArgumentException("Hash stack can't push equal elements!");
            if (first == null)
                first = new HashStackNode<T>(value);
            else
            {
                var newNode = new HashStackNode<T>(value);
                newNode.Next = first;
                first.Previous = newNode;
                first = newNode;
            }
            nodes[value] = first;
        }

        public T Pop()
        {
            if (first == null)
                throw new IndexOutOfRangeException();
            var popResult = first;
            nodes.Remove(popResult.Value);
            first = first.Next;
            if (first != null)
                first.Previous = null;
            return popResult.Value;
        }

        public bool Contains(T value)
        {
            return nodes.ContainsKey(value);
        }

        public void Remove(T value)
        {
            if (!nodes.ContainsKey(value))
                throw new ArgumentException("No such element in stack!");
            var nodeToRemove = nodes[value];
            if (nodeToRemove == first)
                Pop();
            else if (nodeToRemove.Next == null)
            {
                nodeToRemove.Previous.Next = null;
                nodes.Remove(value);
            }
            else
            {
                nodeToRemove.Previous.Next = nodeToRemove.Next;
                nodeToRemove.Next.Previous = nodeToRemove.Previous;
                nodes.Remove(value);
            }
        }

        public void Clear()
        {
            nodes.Clear();
            first = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = first;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}