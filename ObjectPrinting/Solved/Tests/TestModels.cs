namespace ObjectPrinting.Solved.Tests
{
    public class Node
    {
        public string Name { get; set; } = "";
        public Node? Next { get; set; }
    }

    public class TreeNode
    {
        public string Name { get; set; } = "";
        public TreeNode? Father { get; set; }
    }
}