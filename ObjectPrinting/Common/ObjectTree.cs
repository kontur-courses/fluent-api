using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Common
{
    internal class ObjectTreeNode
    {
        public object Value { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public ObjectTreeNode Parent { get; set; }
        public bool EndsLoop { get; set; }
        public List<ObjectTreeNode> Nodes { get; } = new List<ObjectTreeNode>();
    }
}