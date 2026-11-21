using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class TreeNode
{
    public string Name { get; set; }
    public List<TreeNode> Children { get; set; }
    public TreeNode Parent { get; set; }
}
