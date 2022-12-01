using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Common
{
    internal class ObjectTreeNode
    {
        public FieldPropertyObject Object { get; set; }
        public ObjectTreeNode Parent { get; set; }
        public bool EndsLoop { get; set; }
        public List<ObjectTreeNode> Nodes { get; } = new List<ObjectTreeNode>();
    }
}