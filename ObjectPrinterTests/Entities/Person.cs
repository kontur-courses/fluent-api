namespace ObjectPrinterTests.Entities;

public class Person
{
    public string Name { get; set; } = "";
    public int[] Scores { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, Address> Addresses { get; set; } = new();
}