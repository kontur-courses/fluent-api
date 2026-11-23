using NUnit.Framework;

namespace ObjectPrinting.Tests;

internal abstract class PersonFixture
{
    protected Person person;

    [SetUp]
    public void SetUpPerson()
    {
        person = new Person
        {
            Name = "Alex",
            Age = 42,
            Height = 1.85,
            BirthDate = new DateTime(1980, 5, 12),
            Address = new Address { City = "Moscow", Street = "Tverskaya", Building = 7 },
            Scores = new List<int> { 10, 20, 30 },
            Status = Status.Active
        };
    }
}