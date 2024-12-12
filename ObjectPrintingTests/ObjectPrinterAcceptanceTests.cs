using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
[UseReporter(typeof(DiffReporter))]
public class ObjectPrinterAcceptanceTests
{
    private Person person;

    [SetUp]
    public void Setup()
    {
        person = new Person()
        {
            Name = "John",
            Age = 25,
            Height = 123.4,
            Id = new Guid()
        };
    }

    [Test]
    public void ObjectPrinter_WorksCorrect_WithAllConfigs()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding<Guid>()
            .Printing<int>().Using(i => "Hello")
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing(p => p.Name).TrimmedToLength(2)
            .Excluding(p => p.Age);

        var result = printer.PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_HasDefaultConfig()
    {
        var result = person.PrintToString();
        Approvals.Verify(result);
    }
    
    [Test]
    public void ObjectPrinter_HasInlineConfiguration()
    {
        var result = person.PrintToString(s => s.Excluding(p => p.Age));
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Should_ExcludeTypes()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<int>()
            .PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Should_ExcludeProperties()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(p => p.Name)
            .PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Should_CustomizePrintingForType()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<int>().Using(i => $"Hello, {i}")
            .PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Should_CustomizePrintingForProperty()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(p => p.Height).Using(i => $"Hello, {i}")
            .PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Should_CustomizeCultureForIFormattable()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<TimeSpan>().Using(CultureInfo.CurrentCulture)
            .PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Should_TrimStringProperties()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(p => p.Name).TrimmedToLength(1)
            .PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_NotTrhows_WithCycleDependencies()
    {
        var child = new Person
        {
            Name = "Child",
        };
        var parent = new Person
        {
            Name = "John",
            Age = 25,
            Height = 123.4,
            Id = new Guid(),
            Children = [child],
            Dict = new Dictionary<string, int>()
            {
                ["Name"] = 1,
                ["Abc"] = 54,
                ["Key"] = 3,
            }
        };
        child.Children = [parent];
        var action = () => ObjectPrinter.For<Person>().PrintToString(parent);
        action.Should().NotThrow();
    }
    
    [Test]
    public void ObjectPrinter_WorksCorrect_WithCycleDependencies()
    {
        var child = new Person
        {
            Name = "Child",
        };
        var parent = new Person
        {
            Name = "John",
            Age = 25,
            Height = 123.4,
            Id = new Guid(),
            Children = [child],
            Dict = new Dictionary<string, int>()
            {
                ["Name"] = 1,
                ["Abc"] = 54,
                ["Key"] = 3,
            }
        };
        child.Children = [parent];
        var result = ObjectPrinter.For<Person>().PrintToString(parent);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_SerializesList_WhenObjects()
    {
        person.Children = [new Person { Name = "Child1" }, new Person { Name = "Child2" }];
        var result = ObjectPrinter.For<Person>().PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_SerializesList_WhenValueObjects()
    {
        person.Weigths = [1, 2, 3, 4, 5];
        var result = ObjectPrinter.For<Person>().PrintToString(person);
        Approvals.Verify(result);
    }

    [Test]
    public void ObjectPrinter_Serializes_Dictionaries()
    {
        person.Dict = new Dictionary<string, int>()
        {
            ["Name"] = 1,
            ["Age"] = 51,
            ["Children"] = 4,
        };
        var result = ObjectPrinter.For<Person>().PrintToString(person);
        Approvals.Verify(result);
    }
}