namespace ObjectPrintingTests;

public class PersonWithParent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public PersonWithParent Parent { get; set; }
}