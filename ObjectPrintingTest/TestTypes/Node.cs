namespace ObjectPrintingTest.TestTypes;

public class Node
{
    public string Name { get; set; }
    public List<Node> Nodes { get; private set; }

    public Node(string name)
    {
        Name = name;
        Nodes = new List<Node>();
    }

    public Node(string name, List<Node> nodes)
    {
        Name = name;
        Nodes = nodes;
    }

    public Node Add(Node node)
    {
        Nodes.Add(node);
        return this;
    }
}