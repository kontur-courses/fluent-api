using System;
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
        ObjectPrinter.Serialize(person).Should().Be(expected);
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
        ObjectPrinter.Serialize(person,
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
        ObjectPrinter.Serialize(person,
                c => c.Exclude(x => x.Age))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_SameStateDiffRef()
    {
        var first = new Person { Name = "Alex", Age = 19 };
        var second = new Person { Name = "Alex", Age = 19 };

        Person[] persons = [first, second];

        var expected = """
                       Person[]:
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Alex
                       		Height = 0
                       		Age = 19
                       	Person
                       		Id = 00000000-0000-0000-0000-000000000000
                       		Name = Alex
                       		Height = 0
                       		Age = 19

                       """;
        ObjectPrinter.Serialize(persons).Should().Be(expected);
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
        ObjectPrinter.Serialize(person,
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
        ObjectPrinter.Serialize(person,
                c => c.SetCulture<double>(new CultureInfo(culture)))
            .Should().Be(expected);
    }
    
    [TestCase("ru-RU")]
    [TestCase("en-US")]
    public void PrintToString_SetCulture_AffectSpecificIFormattable(string culture)
    {
        var person = new DateTime(1999,12,30);
        var expected = $"""
                        30.12.1999 0:00:00
                        
                        """;
        ObjectPrinter.Serialize(person,
                c => c.SetCulture<double>(new CultureInfo(culture)))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_AlternativePropertySerialization()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Name
                       	Height = 0
                       	Age = 19

                       """;
        ObjectPrinter.Serialize(person,
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
        ObjectPrinter.Serialize(person,
                c => c.TrimStringsToLength(1))
            .Should().Be(expected);
    }

    [Test]
    public void PrintToString_TrimStrings_WhenStringIsSmallerThanLength()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var expected = """
                       Person
                       	Id = 00000000-0000-0000-0000-000000000000
                       	Name = Alex
                       	Height = 0
                       	Age = 19

                       """;
        ObjectPrinter.Serialize(person,
                c => c.TrimStringsToLength(10))
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
        ObjectPrinter.Serialize(person).Should().Be(expected);
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

        ObjectPrinter.Serialize(personList).Should().Be(expected);
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

        ObjectPrinter.Serialize(personArray).Should().Be(expected);
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

        ObjectPrinter.Serialize(personDictionary).Should().Be(expected);
    }
}