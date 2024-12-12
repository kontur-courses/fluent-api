namespace ObjectPrintingTests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public Person[] Children { get; set; }
    public List<int> Weigths { get; set; }
    public Dictionary<string, int> Dict { get; set; }
}