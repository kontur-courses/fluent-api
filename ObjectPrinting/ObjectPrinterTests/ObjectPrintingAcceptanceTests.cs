using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Solved.Tests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ObjectPrinterTests;

public class Collections
{
    public Dictionary<string, string> Dictionary { set; get; }
    public List<object> List { set; get; }
    public int[][] Array { set; get; }
    public List<Person> Persons { set; get; }

    public IEnumerable<int> Enumerable;
}

public class Person
{
    public Guid Id { get; set; } = Guid.Empty;
    public int Age { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
}

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void ExcludingStringProperty_ShouldNotSerializeStringProperty()
    {
        var student = new Person { Name = "Vladimir", Age = 38 };

        var result = ObjectPrinter.For<Person>()
            .Excluding<string>()
            .PrintToString(student);

        result.Should().NotContain(nameof(student.Name));
    }

    [Test]
    public void PrintingWithAlternativeSerialization_ShouldPrinteObjectUsingCustomSerialization()
    {
        var student = new Person { Id = Guid.NewGuid() };

        var result = ObjectPrinter.For<Person>()
            .Printing<Guid>()
            .Using(x => "26afdc28-5331-4c48-b15e-78afe16583d9")
            .PrintToString(student);

        result.Should().Contain("Id = 26afdc28-5331-4c48-b15e-78afe16583d9");
    }

    [Test]
    public void WhenCultureIsSetForProperty_ShouldSerializePropertyWithSpecifiedCulture()
    {
        var student = new Person { Name = "Vladimir", Age = 38, Height = 1.5 };
        var cultureWithComma = new CultureInfo("fr-FR");

        var result = ObjectPrinter.For<Person>()
            .Printing(x => x.Height)
            .Using(cultureWithComma)
            .PrintToString(student);

        result.Should().Contain($"{nameof(student.Height)} = {student.Height.ToString(cultureWithComma)}");
    }

    [Test]
    public void WhenConfiguringProperty_ShouldPrintObjectWithConfiguredProperty()
    {
        var student = new Person { Age = 44 };

        var result = ObjectPrinter.For<Person>()
            .Printing(x => x.Age)
            .Using(age => $"Age: {age}")
            .PrintToString(student);

        Console.WriteLine (result);
        result.Should().Contain("Age = Age: 44");
    }

    [Test]
    public void WhenTrimmedToLength_ShouldPrintObjectWithTrimmedName()
    {
        var student = new Person { Name = "Alex Ovechkin" };

        var result = ObjectPrinter.For<Person>()
            .Printing(x => x.Name)
            .TrimmedToLength(5)
            .PrintToString(student);

        result.Should().Contain("Name = Alex ");
    }

    [Test]
    public void WhenExcludingSpecificProperty_ShouldNotPrintExcludedProperty()
    {
        var student = new Person { Name = "Vladimir", Age = 38 };

        var result = ObjectPrinter.For<Person>()
            .Excluding(x => x.Age)
            .PrintToString(student);

        Console.WriteLine(result);
        result.Should().NotContain("Age");
    }

    [Test]
    public void CorrectlyHandlesCyclicReferences_ShouldNotContainCycle()
    {
        var student = new Person { Name = "Vladimir", Age = 38 };

        var res1 = ObjectPrinter.For<Person>()
            .PrintToString(student);
        var res2 = ObjectPrinter.For<Person>()
            .PrintToString(student);

        res2.Should().NotContain("Cycle");

    }

    [Test]
    public void WhenPrintingAllPublicMembers_ShouldIncludeAllPropertiesAndFields()
    {
        var student = new Person { Name = "Vladimir", Age = 38, Height = 1.9 };
        var result = ObjectPrinter.For<Person>()
            .PrintToString(student);

        var type = student.GetType();
        var propertiesAndFields = type
            .GetMembers()
            .Where(member => member.MemberType == System.Reflection.MemberTypes.Property || member.MemberType == System.Reflection.MemberTypes.Field);

        foreach (var member in propertiesAndFields)
        {
            result.Should().Contain($"{member.Name}");
        }
    }

    [Test]
    public void WhenTrimmedStringProperties_ShouldPrintObjectWithTrimmedProperties()
    {
        const int maxLength = 2;
        var student = new Person { Name = "Vladimir", Age = 38, Height = 1.9, Id = Guid.Empty };

        var result = ObjectPrinter.For<Person>()
            .Printing<string>()
            .TrimmedToLength(maxLength)
            .PrintToString(student);

        Console.WriteLine(result);
        result.Should().Contain($"{nameof(student.Name)} = {student.Name[..maxLength]}");
    }

    [Test]
    public void WhenTrimmedStringPropertiesWithNegativeLength_ShouldThrowArgumentException()
    {
        var student = new Person { Name = "Vladimir", Age = 38, Height = 1.9, Id = Guid.Empty };

        var action = () => { ObjectPrinter.For<Person>()
            .Printing<string>()
            .TrimmedToLength(-44)
            .PrintToString(student); };

        action.Should().Throw<ArgumentException>()
            .WithMessage("Error: Negative trimming length is not allowed");
    }

    [Test]
    public void WhenPrintingDictionaryProperty_ShouldPrintObjectWithDictionaryContentst()
    {
        string newLine = Environment.NewLine;

        var dictionary = new Dictionary<string, string>
        {
            { "France", "Paris" },
            { "Russia", "Moscow"},
            { "Germany", "Berlin" },
            { "Italy", "Rome" },
            { "USA", "Washington" },
            { "South Korea", "Seoul" }
        };

        var collections = new Collections { Dictionary = new Dictionary<string, string>(dictionary) };
        var result = ObjectPrinter.For<Collections>().PrintToString(collections);

        result.Should().Contain($"{nameof(collections.Dictionary)}");

        foreach (var kvp in dictionary)
        {
            result.Should().Contain($"{kvp.Key}{newLine} : {kvp.Value}{newLine}");
        }
    }

    [Test]
    public void PrintingObjectWithListProperty_ShouldPrintObjectWithListContents()
    {
        var collections = new Collections { List = new List<object> { 2, 0, 2, 4 } };
        var result = ObjectPrinter.For<Collections>().PrintToString(collections);

        result.Should().Contain($"{nameof(collections.List)}");

        foreach (var value in collections.List)
        {
            result.Should().Contain($"{value}{Environment.NewLine}");
        }
    }

    [Test]
    public void PrintingObjectWithArrayGenericObjects_ShouldPrintObjectWithArrayContents()
    {
        string newLine = Environment.NewLine;

        var collections = new Collections { Array = new[] { new[] { 2, 0, 2, 4 } } };
        var result = ObjectPrinter.For<Collections>()
            .PrintToString(collections);

        result.Should().Contain($"{nameof(collections.Array)} = {newLine}");

        var arrayContents = collections.Array.First();
        result.Should().Contain($"\t\t{arrayContents.GetType().Name}{newLine}");

        foreach (var value in arrayContents)
        {
            result.Should().Contain($"\t\t\t{value}{newLine}");
        }
    }
}