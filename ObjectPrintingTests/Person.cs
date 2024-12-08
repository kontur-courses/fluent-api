namespace ObjectPrintingTests;

public class Person
{
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public bool IsMarried { get; set; }
    public Person[] Parents { get; set; }
    public Dictionary<string, string> Documents { get; set; }
    public HouseAddress HouseAddress { get; set; }
}