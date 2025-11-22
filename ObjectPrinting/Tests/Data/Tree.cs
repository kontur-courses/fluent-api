namespace ObjectPrinting.Tests.Data;

public class Tree
{
    private int age;
    public Node Root;

    public Tree(Node root, int age)
    {
        Root = root;
        this.age = age;
    }
}