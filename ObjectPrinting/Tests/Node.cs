using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    internal class Node
    {
        public string Name { get; set; }
        public Node Next { get; set; }

        public Node(string name, Node next)
        {
            Name = name;
            Next = next;
        }
    }
}
