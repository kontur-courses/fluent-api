using ObjectPrintingHomework;
using FluentAssertions;
using System.Globalization;

namespace TestsObjectPrinting;
public class TestsObjectPrinting
{
    private Person person;
    [SetUp]
    public void Setup()
    {
        person = new Person { Name = "Milana", Age = 20, Height = 164.4, Surname = "Zinovieva", CountEyes = 2, DateBirth = new DateTime(2004, 09, 19, 12, 30, 45) };
    }

    [Test]
    public void TestOneExcludingType()
    {
        const string notExcepted = nameof(Person.Name) + " = ";
        var result = ObjectPrinter.For<Person>()
            .Excluding<string>()
            .PrintToString(person);

        result.Should().NotContain(notExcepted);
    }

    [Test]
    public void TestAllExcludingTypes()
    {
        const string excepted = "Person";
        var result = ObjectPrinter.For<Person>()
            .Excluding<string>().Excluding<int>().Excluding<double>().Excluding<Guid>().Excluding<DateTime>().Excluding<Person[]>()
            .PrintToString(person);

        result.Should().Be(excepted);
    }

    [Test]
    public void TestExcludingGeneralType()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<Person>()
            .PrintToString(person);

        result.Should().BeEmpty();
    }

    [Test]
    public void TestExcludingProperty()
    {
        const string notExcepted = nameof(Person.Age) + " = ";
        var result = ObjectPrinter.For<Person>()
            .Excluding(p => p.Age)
            .PrintToString(person);

        result.Should().NotContain(notExcepted);
    }

    [Test]
    public void TestExcludingAllProperties()
    {
        const string excepted = "Person";
        var result = ObjectPrinter.For<Person>()
            .Excluding(p => p.Age).Excluding(p => p.CountEyes).Excluding(p => p.Height)
            .Excluding(p => p.Id).Excluding(p => p.Name).Excluding(p => p.Surname).Excluding(p => p.DateBirth).Excluding<Person[]>()
            .PrintToString(person);

        result.Should().Be(excepted);
    }

    [Test]
    public void TestAnotherSerializationForString()
    {
        const string excepted = " serialization";
        var result = ObjectPrinter.For<Person>().Printing<string>().Using(p => p + " serialization")
            .PrintToString(person);

        result.Should().Contain(excepted);
    }

    [Test]
    public void TestAnotherSerializationForInt()
    {
        const string exceptedForAge = "Age = 0";
        const string exceptedForEyes = "CountEyes = 0";
        var result = ObjectPrinter.For<Person>().Printing<int>().Using(p => "0")
            .PrintToString(person);

        result.Should().Contain(exceptedForAge).And.Contain(exceptedForEyes);
    }

    [Test]
    public void TestSetCultureForDouble()
    {
        const string excepted = "164.4";
        const string unexcepted = "164,4";
        var result = ObjectPrinter.For<Person>().Printing<double>().Using(i => new CultureInfo("en-GB"))
            .PrintToString(person);

        result.Should().NotContain(unexcepted).And.Contain(excepted);
    }

    [Test]
    public void TestSetCultureForDateTime()
    {
        const string unexcepted = "19.09.2004 12:30:45";
        const string excepted = "19/09/2004 12:30:45";
        var result = ObjectPrinter.For<Person>().Printing<DateTime>().Using(i => new CultureInfo("fr"))
            .PrintToString(person);

        result.Should().NotContain(unexcepted).And.Contain(excepted);
    }

    [Test]
    public void TestAnotherSerializationForName()
    {
        const string unexcepted = "Milana";
        const string excepted = "MILANA";
        var result = ObjectPrinter.For<Person>()
            .Printing<string>(p => p.Name)
            .Using(p => p.ToUpper())
            .PrintToString(person);

        result.Should().NotContain(unexcepted).And.Contain(excepted);
    }

    [Test]
    public void TestTrimmingSurname()
    {
        const string unexcepted = "Zinovieva";
        const string excepted = "Zin";
        var result = ObjectPrinter.For<Person>()
            .Printing<string>(p => p.Surname).TrimmedToLength(3)
            .PrintToString(person);

        result.Should().NotContain(unexcepted).And.Contain(excepted);
    }

    [Test]
    public void TestExceptionForTrimming()
    {
        Action action = () => ObjectPrinter.For<Person>()
            .Printing<int>(p => p.Age)
            .TrimmedToLength(3);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Trimming is only supported for string properties");
    }

    [Test]
    public void Work_WhenReferenceCycles()
    {
        const string excepted = "Circular Reference";
        person.Parents = [person];
        person.Friends = [person];
        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Console.WriteLine(result);
        result.Should().Contain(excepted);
    }
}