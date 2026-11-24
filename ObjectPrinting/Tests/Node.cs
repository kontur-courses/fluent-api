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
