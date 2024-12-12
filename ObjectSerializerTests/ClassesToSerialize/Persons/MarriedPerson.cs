namespace ObjectSerializerTests.ClassesToSerialize.Persons;

public class MarriedPerson : Person
{
    public Person? Partner { get; private set; }

    public MarriedPerson(string name, double height, int age)
        : this(Guid.NewGuid(), name, height, age)
    {
    }

    public MarriedPerson(Guid id, string name, double height, int age)
        : base(id, name, height, age)
    {
    }

    public void SetPartner(Person partner)
    {
        Partner = partner;
    }
}