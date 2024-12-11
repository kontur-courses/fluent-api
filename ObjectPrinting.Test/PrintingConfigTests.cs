using System.Globalization;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting.Test;

public class TestsObjectPrinting
{
    private Person person;

    [SetUp]
    public void Setup()
    {
        person = new()
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
    }

    [Test]
    public void Exclude_ExcludeType()
    {
        const string unexpectedName = nameof(Person.Name);
        const string unexpectedSurname = nameof(Person.Name);
        var result = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .PrintToString(person);

        result.Should().NotContain(unexpectedName).And.NotContain(unexpectedSurname);
    }

    [Test]
    public void Exclude_ExcludeProperty()
    {
        const string unexpected = nameof(Person.Age);
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age)
            .PrintToString(person);

        result.Should().NotContain(unexpected);
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
            .PrintToString(person);

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
            .PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void Using_SerializationForString()
    {
        const string expected = " is beautiful";
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>()
            .Using(p => $"{p} is beautiful")
            .PrintToString(person);

        result.Should().Contain(expected);
    }

    [Test]
    public void Using_SerializationForInt()
    {
        const string exceptedAge = "Age = 10";
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<int>()
            .Using(_ => "10")
            .PrintToString(person);

        result.Should().Contain(exceptedAge);
    }

    [Test]
    public void Using_SerializationForName()
    {
        var expected = person.Name?.ToUpper();
        var unexpected = person.Name;
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .Using(p => p!.ToUpper())
            .PrintToString(person);

        result.Should().Contain(expected).And.NotContain(unexpected);
    }

    [Test]
    public void UseCulture_ChangeCultureForDouble()
    {
        var expected = person.Height.ToString(new CultureInfo("ru-RU"));
        var unexpected = person.Height.ToString(new CultureInfo("en-US"));
        var result = ObjectPrinter.For<Person>()
            .UseCulture<double>(new("ru-RU"))
            .PrintToString(person);

        result.Should().Contain(expected).And.NotContain(unexpected);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void TrimmedTo_TrimmingName(int length)
    {
        var expected = $"{nameof(person.Name)} = {person.Name?[..length]}";
        var unexpected = person.Name;
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .TrimmedTo(length)
            .PrintToString(person);

        result.Should().Contain(expected).And.NotContain(unexpected);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void SetMaxNestingLevel_WithDifferentNestingLevels(int levelNesting)
    {
        var expected = $"Достигнут максимум глубины рекурсии: {levelNesting}.";
        person.BestFriend = person;
        person.Friends = [person];
        var result = ObjectPrinter.For<Person>()
            .SetMaxNestingLevel(levelNesting)
            .PrintToString(person);

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
                .Should().Throw<ArgumentException>()
                .WithMessage("Max nesting level must be greater than or equal to 0.");
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
            .PrintToString(person);
        
        result.Should().Contain(v2);
        Console.WriteLine(v2);
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
            .PrintToString(person);
        
        result.Should().Contain(v2);
        Console.WriteLine(v2);
    }
}