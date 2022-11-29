using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class PrintingConfigTests
{
    [SetUp]
    public void Setup()
    {
        _person = new Person
        {
            Name = "Johnny", Surname = "Silverhand", Height = 183.5, Weight = 70.75f, Iq = 200,
            DateOfBirth = new DateTime(1988, 11, 16)
        };
    }

    private Person _person;

    [Test]
    public void PrintingObjectWithoutExplicitConfig()
    {
        var printedObject = _person.PrintToString();
        printedObject.Should().ContainAll(
            "Name = Johnny",
            "Surname = Silverhand",
            "DateOfBirth = 16.11.1988 0:00:00",
            "Father = null",
            "Mother = null",
            "BestFriend = null",
            "Height = 183,5",
            "Weight = 70,75",
            "Iq = 200");
    }

    [Test]
    public void PrintingConfigExcludeStrings()
    {
        var config = ObjectPrinter.For<Person>().Excluding<string>();
        var printedObject = _person.PrintToString(config);
        printedObject.Should().NotContainAll(
            "Name = Johnny",
            "Surname = Silverhand");
    }

    [Test]
    public void PrintingConfigExcludePersons()
    {
        var config = ObjectPrinter.For<Person>().Excluding<Person>();
        var printedObject = _person.PrintToString(config);
        printedObject.Should().NotContainAll(
            "Father = null",
            "Mother = null",
            "BestFriend = null");
    }

    [Test]
    public void PrintingConfigExcludeMemberByName()
    {
        var config = ObjectPrinter.For<Person>().Excluding(person => person.DateOfBirth);
        var printedObject = _person.PrintToString(config);
        printedObject.Should().NotContainAll(
            "DateOfBirth = 16.11.1988 0:00:00");
    }

    [Test]
    public void PrintingConfigAlternativePrintingForStrings()
    {
        var config = ObjectPrinter.For<Person>().Printing<string>().Using(str => str.ToLower());
        var printedObject = _person.PrintToString(config);
        printedObject.Should().ContainAll(
            "Name = johnny",
            "Surname = silverhand");
    }

    [Test]
    public void PrintingConfigAlternativePrintingForMemberByName()
    {
        var config = ObjectPrinter.For<Person>().Printing(person => person.Name).Using(name => name.ToUpper());
        var printedObject = _person.PrintToString(config);
        printedObject.Should().ContainAll(
            "Name = JOHNNY");
    }

    [Test]
    public void PrintingConfigTrimStringMember()
    {
        var config = ObjectPrinter.For<Person>().Printing<string>().TrimmedToLength(4);
        var printedObject = _person.PrintToString(config);
        printedObject.Should().ContainAll(
            "Name = John",
            "Surname = Silv");
    }

    [Test]
    public void PrintingConfigSetCultureForDouble()
    {
        var config = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);
        var printedObject = _person.PrintToString(config);
        printedObject.Should().ContainAll(
            "Height = 183.5");
    }

    [Test]
    public void PrintingConfigShouldNotGoInRecursion()
    {
        var father = new Person { Name = "Ilya", Surname = "Silverhand" };
        _person.Father = father;
        father.BestFriend = _person;

        Func<string> printingObject = () => _person.ToString();
        printingObject.Should().NotThrow<StackOverflowException>();
    }
}