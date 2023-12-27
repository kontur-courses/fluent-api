namespace ObjectPrinting.Tests;

public class BasePerson
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public int[] Array { get; set; }
    
    public static BasePerson Get()
    {
        return new BasePerson
        {
            Id = new Guid(),
            Age = 20,
            Height = 183.4,
            Name = "Peter",
            Array = new []{2, 3, 4},
        };
    }
}