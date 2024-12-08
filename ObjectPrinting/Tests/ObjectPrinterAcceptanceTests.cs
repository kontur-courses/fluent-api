using NUnit.Framework;
using FluentAssertions;
using System.Globalization;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void PrintToString_Serialize()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Alex
                       	Height = 0
                       	Age = 19

                       """;
        person.Serialize().Should().Be(expected);
    }

    [Test]
    public void PrintToString_ExcludeType()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Alex
                       	Height = 0

                       """;
        person.Serialize(
                c => c.Exclude<int>())
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_ExcludeProperty()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Alex
                       	Height = 0

                       """;
        person.Serialize(
                c => c.Exclude(x => x.Age))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_AlternativeTypeSerialization()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Alex
                       	Height = 0
                       	Age = !

                       """;
        person.Serialize(
                c => c.SetCustomSerialization<int>(_ => "!"))
            .Should().Be(expected);
    }

    [TestCase("ru-RU", ",")]
    [TestCase("en-US", ".")]
    public void PrintToString_SetCulture(string culture, string expectedSeparator)
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 110.1 };
        var expected = $"""
                        Person
                        	Id = 00000000-0000-0000-0000-000000000000
                        	Name = Alex
                        	Height = 110{expectedSeparator}1
                        	Age = 19

                        """;
        person.Serialize(
                c => c.SetCulture(new CultureInfo(culture)))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_AlternativePropetrySerialization()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Name
                       	Height = 0
                       	Age = 19

                       """;
        person.Serialize(
                c => c.SetCustomSerialization(x => x.Name, _ => "Name"))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_TrimStrings()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = A
                       	Height = 0
                       	Age = 19

                       """;
        person.Serialize(
                c => c.TrimStringsToLength(1))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_CircularReference()
    {
        var person = new PersonV2 { Name = "Alex", Age = 19 };
        person.Mother = person;

        var expected = """
                       PersonV2
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Alex
                       	Height = 0
                       	Age = 19
                       	Mother = [Circular Reference]
                       	Kids = null

                       """;
        person.Serialize().Should().Be(expected);
    }

    [Test]
    public void PrintToString_SerializeList()
    {
        List<Person> personList =
        [
            new() { Name = "Alex", Age = 19 },
            new() { Name = "Brian", Age = 10 },
            new() { Name = "Steve", Age = 99 },
        ];

        var expected = """
                       List`1:
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Alex
                       		Height = 0
                       		Age = 19
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Brian
                       		Height = 0
                       		Age = 10
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Steve
                       		Height = 0
                       		Age = 99

                       """;

        personList.Serialize().Should().Be(expected);
    }

    [Test]
    public void PrintToString_SerializeArray()
    {
        Person[] personArray =
        [
            new() { Name = "Alex", Age = 19 },
            new() { Name = "Brian", Age = 10 },
            new() { Name = "Steve", Age = 99 },
        ];

        var expected = """
                       Person[]:
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Alex
                       		Height = 0
                       		Age = 19
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Brian
                       		Height = 0
                       		Age = 10
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Steve
                       		Height = 0
                       		Age = 99

                       """;

        personArray.Serialize().Should().Be(expected);
    }

    [Test]
    public void PrintToString_SerializeDictionary()
    {
        Dictionary<Person, int> personDictionary = new()
        {
            { new() { Name = "Alex", Age = 19 }, 3 },
            { new() { Name = "Brian", Age = 10 }, 2 },
            { new() { Name = "Steve", Age = 99 }, 1 }
        };

        var expected = """
                       Dictionary`2:
                       	KeyValuePair`2
                       		Key = Person
                       			Id = 00000000-0000-0000-0000-000000000000
                       			Name = Alex
                       			Height = 0
                       			Age = 19
                       		Value = 3
                       	KeyValuePair`2
                       		Key = Person
                       			Id = 00000000-0000-0000-0000-000000000000
                       			Name = Brian
                       			Height = 0
                       			Age = 10
                       		Value = 2
                       	KeyValuePair`2
                       		Key = Person
                       			Id = 00000000-0000-0000-0000-000000000000
                       			Name = Steve
                       			Height = 0
                       			Age = 99
                       		Value = 1

                       """;

        personDictionary.Serialize().Should().Be(expected);
    }
}