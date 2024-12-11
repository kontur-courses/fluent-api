namespace ObjectPrintingTests;

public class Person(Guid id, string name, double height, int age, 
                    Person? friend = null, List<Person>? friends = null, 
                    Person[]? relatives = null, Dictionary<int, Person>? neighbours = null)
{
    public Person() 
        : this(new Guid(), "Anton", 200.52, 19)
    {
    }

    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public double Height { get; } = height;
    public int Age { get; } = age;

    public Person? Friend { get; set; } = friend;

    public List<Person>? Friends { get; set; } = friends;

    public Person[]? Relatives { get; set; } = relatives;
    public Dictionary<int, Person>? Neighbours { get; set; } = neighbours;

    public readonly int Field = 10;
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, Height, Age, Field);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj?.GetType() != GetType()) return false;
        var person = (Person)obj;
        return person.Name == Name && person.Age == Age;

    }
    
}