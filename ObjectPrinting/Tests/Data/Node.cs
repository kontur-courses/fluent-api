namespace ObjectPrinting.Tests.Data;

public class Node
{
    public Node(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
    public Node Child { get; set; }
}