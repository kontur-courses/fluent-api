namespace ObjectPrintingTests.TestData;

public class Person
{
    public Guid Id { get; }
    public string Name { get; }
    public int Age { get; }
    public double Height { get; }
    
    public Person(Guid id, string name, double height, int age)
    {
        Id = id;
        Name = name;
        Height = height;
        Age = age;
    }
}