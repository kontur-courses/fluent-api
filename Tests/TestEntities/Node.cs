namespace Tests.TestEntities;

public class Node
{
    public string Value { get; set; } = null!;
    public Node? Next { get; set; }

    public static Node CreateChain(int length)
    {
        var parent = new Node { Value = "level0" };
        var node = parent;
        
        for (int i = 0; i < length; i++)
        {
            var nextNode = new Node { Value = $"level{i + 1}"};
            node.Next = nextNode;
            node = nextNode;   
        }
        return parent;
    }

    public static Node CreateChainWithCycle()
    {
        var node1 = new Node { Value = "0" };
        var node2 = new Node { Value = "1" };
        node1.Next = node2;
        node2.Next = node1;
        return node1;
    }
}