namespace ObjectPrintingTests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }

    public Person Parent1 { get; set; }
    public Person Parent2 { get; set; }

    public int[] SomeArray = new[] { 1, 2, 3 };

    protected bool Equals(Person other) => Name == other.Name;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Person)obj);
    }

    public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;
}