namespace Tests.TestEntities;

public class CollectionTestsClass
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public List<CollectionTestsClass> Friends { get; set; } = new();
}