namespace ObjectPrintingTests;

public class PersonWithRoots
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public PersonWithRoots? Root { get; set; }
    public List<PersonWithRoots>? Childrens { get; set; }
}