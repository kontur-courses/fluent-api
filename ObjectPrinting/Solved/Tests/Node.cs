namespace ObjectPrinting.Solved.Tests
{
    public class Node
    {
        public string Name { get; set; }
        public Node nextNode { get; set; }
        public Node(string name) => Name = name;
    }
}