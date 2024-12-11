using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrintingTests.Models;

namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    private string expectedString;
    [SetUp]
    public void Setup()
    {
        expectedString = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180.5, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>();

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithCultureSetForProp()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180.5, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .SetCultureFor(p => p.Height, CultureInfo.GetCultureInfo("ru-RU"));

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithCultureSetForType()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180.5, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .SetCultureFor<double>(CultureInfo.GetCultureInfo("ru-RU"));

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithTrim()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .TrimString(p => p.Name, 2);

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithAlternativeSerialization_ForProperty()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .Serialize<string>(p => p.Name).With(name => name.ToUpper());

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithAlternativeSerializationForType()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
                .Serialize<double>().With(d => d.ToString("F2"))
            ;

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedType()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<double>();

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedProperty()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name);

        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedNestedProperty()
    {
        var person = new Child()
        {
            Name = "Alex", Age = 19, Height = 180, Id = new Guid(),
            Parent = new Person { Name = "John", Age = 50, Height = 190, Id = new Guid() }
        };
        var printer = ObjectPrinter.For<Child>()
            .Exclude(p => p.Parent.Name);
        printer.PrintToString(person).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeCollection()
    {
        var persons = new List<Person>
        {
            new() { Name = "Alex", Age = 19, Height = 180, Id = new Guid() },
            new() { Name = "John", Age = 20, Height = 190, Id = new Guid() }
        };

        var printer = ObjectPrinter.For<List<Person>>();

        printer.PrintToString(persons).Should()
            .Be(
                expectedString);
    }
    [Test]
    public void ObjectPrinter_Should_SerializeDictionary()
    {
        var dictionary = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 }
        };
        var printer = ObjectPrinter.For<Dictionary<string, int>>();
        printer.PrintToString(dictionary).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_HandleRecursion()
    {
        var child = new Child()
        {
            Name = "Alex", Age = 19, Height = 180, Id = new Guid(),
        };
        child.Parent = child;
        var printer = ObjectPrinter.For<Child>();
        printer.PrintToString(child).Should()
            .Be(expectedString);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeArray()
    {
        var people = new Person[]
        {
            new() { Name = "Alex", Age = 19, Height = 180, Id = new Guid() },
            new() { Name = "John", Age = 20, Height = 190, Id = new Guid() }
        };
        var printer = ObjectPrinter.For<Person[]>();
        printer.PrintToString(people).Should()
            .Be(expectedString);
    }

}