namespace ObjectPrintingTests.Domain;

public record Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public Person Parent { get; set; }
    public Person[] Friends { get; set; } = [];
    public bool IsStudent { get; set; }
    public DateTime Birthday { get; set; }
}