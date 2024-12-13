using System.Globalization;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting.Test;

public class TestsObjectPrinting
{
    private Person firstPerson;
    private Person secondPerson;
    private Family family;

    [SetUp]
    public void Setup()
    {
        firstPerson = new()
        {
            Name = "Ben",
            Surname = "Big",
            Height = 170.1,
            Age = 20,
            BestFriend = new() { Name = "Bob", Surname = "Boby", Height = 40, Age = 80 },
            Friends =
            [
                new() { Name = "Alice", Surname = "Sev", Height = 50, Age = 30 },
                new() { Name = "Max", Surname = "Albor", Height = 10, Age = 9 }
            ],
            BodyParts = { { "Hand", 2 }, { "Foot", 2 }, { "Head", 1 }, { "Tail", 0 } }
        };

        secondPerson = new();

        family = new() { Mom = firstPerson, Dad = secondPerson, Children = [firstPerson, secondPerson] };
    }

    [Test]
    public void Exclude_ExcludeType()
    {
        const string unexpectedName = nameof(Person.Name);
        const string unexpectedSurname = nameof(Person.Surname);
        var result = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .PrintToString(firstPerson);

        result.Should().NotContain(unexpectedName).And.NotContain(unexpectedSurname);
    }

    [Test]
    public void Exclude_ExcludeProperty()
    {
        const string unexpectedAge = nameof(Person.Age);
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age)
            .PrintToString(firstPerson);

        result.Should().NotContain(unexpectedAge);
    }

    [Test]
    public void Exclude_AddSerializationToType_ThenExcludeType()
    {
        var unexpectedName = $"{firstPerson.Name} is beautiful";
        var unexpectedSurname = $"{firstPerson.Surname} is beautiful";
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>().Using(p => $"{p} is beautiful")
            .Exclude<string>()
            .PrintToString(firstPerson);

        result.Should().NotContain(unexpectedName).And.NotContain(unexpectedSurname);
    }

    [Test]
    public void Exclude_ExcludeAllProperties()
    {
        const string expected = "Person\r\n";
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Id)
            .Exclude(p => p.Name)
            .Exclude(p => p.Surname)
            .Exclude(p => p.Height)
            .Exclude(p => p.Age)
            .Exclude(p => p.BestFriend)
            .Exclude(p => p.Friends)
            .Exclude(p => p.BodyParts)
            .PrintToString(firstPerson);

        result.Should().Be(expected);
    }

    [Test]
    public void Exclude_ExcludeAllTypes()
    {
        const string expected = "Person\r\n";
        var result = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .Exclude<string>()
            .Exclude<double>()
            .Exclude<int>()
            .Exclude<Person>()
            .Exclude<Person[]>()
            .Exclude<Dictionary<string, int>>()
            .PrintToString(firstPerson);

        result.Should().Be(expected);
    }

    [Test]
    public void Using_SerializationForString()
    {
        const string expected = " is beautiful";
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>()
            .Using(p => $"{p} is beautiful")
            .PrintToString(firstPerson);

        result.Should().Contain(expected);
    }

    [Test]
    public void Using_SerializationForInt()
    {
        const string exceptedAge = $"{nameof(firstPerson.Age)} = 10";
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<int>()
            .Using(_ => "10")
            .PrintToString(firstPerson);

        result.Should().Contain(exceptedAge);
    }

    [Test]
    public void Using_SerializationForName()
    {
        var expected = firstPerson.Name?.ToUpper();
        var unexpected = firstPerson.Name;
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .Using(p => p!.ToUpper())
            .PrintToString(firstPerson);

        result.Should().Contain(expected).And.NotContain(unexpected);
    }

    [Test]
    public void Using_MultiplePropertySerializations()
    {
        var exceptedAge = $"{nameof(firstPerson.Name)} = 21{firstPerson.Name}12";
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .Using(name => $"1{name}1")
            .PrintPropertySettings(p => p.Name)
            .Using(name => $"2{name}2")
            .PrintToString(firstPerson);

        result.Should().Contain(exceptedAge);
    }

    [Test]
    public void Using_MultipleTypeSerializationsForString()
    {
        var exceptedName = $"{nameof(firstPerson.Name)} = 21{firstPerson.Name}12";
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>()
            .Using(str => $"1{str}1")
            .PrintSettings<string>()
            .Using(str => $"2{str}2")
            .PrintToString(firstPerson);

        result.Should().Contain(exceptedName);
    }

    [Test]
    public void Using_TrimmedToPropertiesAfterSerializationForName([Values(0, 1, 2)] int length)
    {
        var exceptedName = $"{nameof(firstPerson.Name)} = " + $"1{firstPerson.Name!}"[..length];
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .Using(name => $"1{name}1")
            .PrintPropertySettings(p => p.Name)
            .TrimmedTo(length)
            .PrintToString(firstPerson);

        result.Should().Contain(exceptedName);
    }

    [Test]
    public void UseCulture_ChangeCultureForDouble()
    {
        var expected = firstPerson.Height.ToString(new CultureInfo("ru-RU"));
        var unexpected = firstPerson.Height.ToString(new CultureInfo("en-US"));
        var result = ObjectPrinter.For<Person>()
            .UseCulture<double>(new("ru-RU"))
            .PrintToString(firstPerson);

        result.Should().Contain(expected).And.NotContain(unexpected);
    }

    [Test]
    public void TrimmedTo_TrimmingName([Values(0, 1, 2)] int length)
    {
        var expected = $"{nameof(firstPerson.Name)} = {firstPerson.Name?[..length]}";
        var unexpected = firstPerson.Name;
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .TrimmedTo(length)
            .PrintToString(firstPerson);

        result.Should().Contain(expected).And.NotContain(unexpected);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void SetMaxNestingLevel_WithDifferentNestingLevels(int levelNesting)
    {
        firstPerson.BestFriend = secondPerson;
        var expected = $"The maximum recursion depth has been reached: {levelNesting}.";
        var result = ObjectPrinter.For<Person>()
            .SetMaxNestingLevel(levelNesting)
            .PrintToString(firstPerson);

        result.Should().Contain(expected);
    }

    [TestCase(-1)]
    [TestCase(-2)]
    [TestCase(-3)]
    [TestCase(-4)]
    [TestCase(-5)]
    public void SetMaxNestingLevel_WithNegativeNestingLevel(int levelNesting)
    {
        FluentActions.Invoking(() => ObjectPrinter.For<Person>()
                .SetMaxNestingLevel(levelNesting))
            .Should().Throw<ArgumentException>();
    }

    [Test]
    public void PrintToString_ProcessingCyclicLinksBetweenObjects()
    {
        firstPerson.BestFriend = secondPerson;
        secondPerson.BestFriend = firstPerson;

        var result = ObjectPrinter.For<Person>()
            .PrintToString(firstPerson);
        result.Should().Contain("It is not possible to print an object with a circular reference.");
    }

    [Test]
    public void PrintToString_WhenLinkIsSame_ButThereAreNoCircularReferences_Array()
    {
        var result = ObjectPrinter.For<Person[]>()
            .PrintToString([firstPerson, firstPerson]);

        result.Should().NotContain("It is not possible to print an object with a circular reference.");
    }

    [Test]
    public void PrintToString_WhenLinkIsSame_ButThereAreNoCircularReferences_List()
    {
        var result = ObjectPrinter.For<List<Person>>()
            .PrintToString([firstPerson, firstPerson]);

        result.Should().NotContain("It is not possible to print an object with a circular reference.");
    }

    [Test]
    public void PrintToString_WhenLinkIsSame_ButThereAreNoCircularReferences_Dictionary()
    {
        var result = ObjectPrinter.For<Dictionary<Person, Person>>()
            .PrintToString(new() { { firstPerson, firstPerson } });

        result.Should().NotContain("It is not possible to print an object with a circular reference.");
    }

    [Test]
    public void PrintToString_WhenLinkIsSame_ButThereAreNoCircularReferences_InsideObject()
    {
        family = family with { Mom = firstPerson, Dad = firstPerson };
        var result = ObjectPrinter.For<Family>()
            .PrintToString(family);

        result.Should().NotContain("It is not possible to print an object with a circular reference.");
    }

    [Test]
    public void PrintToString_CorrectStructure()
    {
        var expectedResult = new StringBuilder();
        expectedResult.Append("Person\r\n\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t" +
            "Name = \"Ben\"\r\n\t" +
            "Surname = \"Big\"\r\n\t" +
            "Height = 170,1\r\n\t" +
            "Age = 20\r\n\t" +
            "BestFriend = Person\r\n\t\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t\t" +
            "Name = \"Bob\"\r\n\t\t" +
            "Surname = \"Boby\"\r\n\t\t" +
            "Height = 40\r\n\t\t" +
            "Age = 80\r\n\t\t" +
            "BestFriend = null\r\n\t\t" +
            "Friends = {}\r\n\t\t" +
            "BodyParts = {}\r\n\r\n\t" +
            "Friends = {\r\n\t\t" +
            "Person\r\n\t\t\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t\t\t" +
            "Name = \"Alice\"\r\n\t\t\t" +
            "Surname = \"Sev\"\r\n\t\t\t" +
            "Height = 50\r\n\t\t\t" +
            "Age = 30\r\n\t\t\t" +
            "BestFriend = null\r\n\t\t\t" +
            "Friends = {}\r\n\t\t\t" +
            "BodyParts = {}\r\n\t\t,\r\n\t\t" +
            "Person\r\n\t\t\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t\t\t" +
            "Name = \"Max\"\r\n\t\t\t" +
            "Surname = \"Albor\"\r\n\t\t\t" +
            "Height = 10\r\n\t\t\t" +
            "Age = 9\r\n\t\t\t" +
            "BestFriend = null\r\n\t\t\t" +
            "Friends = {}\r\n\t\t\t" +
            "BodyParts = {}\r\n\t\t" +
            "}\r\n\r\n\t" +
            "BodyParts = {\r\n\t\t\"Hand\": 2\r\n\t\t\"Foot\": 2\r\n\t\t\"Head\": 1\r\n\t\t\"Tail\": 0\r\n\t\t}\r\n\r\n");

        var result = expectedResult.ToString();
        var v2 = ObjectPrinter.For<Person>()
            .PrintToString(firstPerson);

        result.Should().Contain(v2);
    }

    [Test]
    public void PrintToString_WithExcludedProperiesStructure()
    {
        var expectedResult = new StringBuilder();
        expectedResult.Append("Person\r\n\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t" +
            "Surname = \"Big\"\r\n\t" +
            "Height = 170,1\r\n\t" +
            "BestFriend = Person\r\n\t\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t\t" +
            "Surname = \"Boby\"\r\n\t\t" +
            "Height = 40\r\n\t\t" +
            "BestFriend = null\r\n\t\t" +
            "Friends = {}\r\n\t\t" +
            "BodyParts = {}\r\n\r\n\t" +
            "Friends = {\r\n\t\t" +
            "Person\r\n\t\t\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t\t\t" +
            "Surname = \"Sev\"\r\n\t\t\t" +
            "Height = 50\r\n\t\t\t" +
            "BestFriend = null\r\n\t\t\t" +
            "Friends = {}\r\n\t\t\t" +
            "BodyParts = {}\r\n\t\t,\r\n\t\t" +
            "Person\r\n\t\t\t" +
            "Id = 00000000-0000-0000-0000-000000000000\r\n\t\t\t" +
            "Surname = \"Albor\"\r\n\t\t\t" +
            "Height = 10\r\n\t\t\t" +
            "BestFriend = null\r\n\t\t\t" +
            "Friends = {}\r\n\t\t\t" +
            "BodyParts = {}\r\n\t\t" +
            "}\r\n\r\n\t" +
            "BodyParts = {\r\n\t\t\"Hand\": 2\r\n\t\t\"Foot\": 2\r\n\t\t\"Head\": 1\r\n\t\t\"Tail\": 0\r\n\t\t}\r\n\r\n");

        var result = expectedResult.ToString();
        var v2 = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name)
            .Exclude(p => p.Age)
            .PrintToString(firstPerson);

        result.Should().Contain(v2);
    }
}