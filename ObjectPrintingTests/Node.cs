using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrintingTests
{
    public class Node
    {
        public Node OtherNode { get; set; }

        public double Value { get; set; }

        public Node()
        {
            Value = 0;
        }

        public Node(double v)
        {
            Value = v;
        }

        public void AddNode(Node other)
        {
            OtherNode = other;
            other.OtherNode = this;
        }

    }
}
