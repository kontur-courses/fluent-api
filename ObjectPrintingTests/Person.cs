using FluentAssertions.Equivalency;

namespace ObjectPrintingTests;

public class Person
{
    
    public int Age { get; set; }
    public Guid Id { get; set; }
    public Person Friend { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Height, Age, 10);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj?.GetType() != GetType()) return false;
        var person = (Person)obj;
        return person.Name == Name && person.Age == Age;

    }
    
}