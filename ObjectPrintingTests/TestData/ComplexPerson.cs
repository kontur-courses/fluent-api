namespace ObjectPrintingTests.TestData;

public class ComplexPerson : Person
{
    public ComplexPerson? Parent { get; set; }

    public float Weight = 0;
    
    public ComplexPerson(Guid id, string name, double height, int age) : base(id, name, height, age)
    {
    }
}