namespace ObjectPrinting.Tests
{
    public class TreeNode
    {
        public int Id { get; set; }
        public TreeNode LeftNode { get; set; }
        public TreeNode RightNode { get; set; }

        public TreeNode(int id)
        {
            Id = id;
        }
    }
}