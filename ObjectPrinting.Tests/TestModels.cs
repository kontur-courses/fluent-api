namespace ObjectPrinting.Tests;

internal enum Status { Active, Blocked }

internal class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public double Height { get; set; }
    public DateTime BirthDate { get; set; }
    public Status Status { get; set; }
    public Address Address { get; set; }
    public List<int> Scores { get; set; }
}

internal class Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public int Building { get; set; }
}

internal class WithDictionary
{
    public Dictionary<string, int> Map { get; set; }
}

internal class Node
{
    public string Name { get; set; }
    public Node Next { get; set; }
}

internal class TwoAddresses
{
    public Address First { get; set; }
    public Address Second { get; set; }
}