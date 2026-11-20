using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Solved.Tests.Entity
{
    public class Node
    {
        public string Name { get; set; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; } = new();
    }
}
