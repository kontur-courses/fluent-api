namespace Tests.TestEntities;

public class CustomSerializerTestsClass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!; 
    public int Age { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}