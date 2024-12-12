using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrintingTests.Models;

namespace ObjectPrintingTests;

[Parallelizable(ParallelScope.All)]
public class ObjectPrinterTests
{
    [Test]
    public void ObjectPrinter_Should_SerializeObject()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180.5, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>();

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithCultureSet_ForProperty()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180.5, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .SetCultureFor(p => p.Height, CultureInfo.GetCultureInfo("ru-RU"));

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithCultureSet_ForType()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180.5, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .SetCultureFor<double>(CultureInfo.GetCultureInfo("ru-RU"));

        printer.PrintToString(person).Should()
            .Be(expected);
    }


    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithTrim()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .TrimString(p => p.Name, 2);

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithAlternativeSerialization_ForProperty()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .Serialize<string>(p => p.Name).With(name => name.ToUpper());

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithAlternativeSerialization_ForField()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new SkilledPerson() { Name = "Alex", Age = 19, Height = 180, Id = new Guid(), Skill = "csharp" };

        var printer = ObjectPrinter.For<SkilledPerson>()
            .Serialize<string>(p => p.Skill).With(skill => skill.ToUpper());

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithAlternativeSerialization_ForType()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
                .Serialize<double>().With(d => d.ToString("F2"))
            ;

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedType()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<double>();

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedProperty()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name);

        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedNestedProperty()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new Child()
        {
            Name = "Alex", Age = 19, Height = 180, Id = new Guid(),
            Parent = new Person { Name = "John", Age = 50, Height = 190, Id = new Guid() }
        };
        var printer = ObjectPrinter.For<Child>()
            .Exclude(p => p.Parent.Name);
        printer.PrintToString(person).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeCollection()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var persons = new List<Person>
        {
            new() { Name = "Alex", Age = 19, Height = 180, Id = new Guid() },
            new() { Name = "John", Age = 20, Height = 190, Id = new Guid() }
        };

        var printer = ObjectPrinter.For<List<Person>>();

        printer.PrintToString(persons).Should()
            .Be(
                expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeDictionary()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var dictionary = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 }
        };
        var printer = ObjectPrinter.For<Dictionary<string, int>>();
        printer.PrintToString(dictionary).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_HandleRecursion()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var child = new Child()
        {
            Name = "Alex", Age = 19, Height = 180, Id = new Guid(),
        };
        child.Parent = child;
        var printer = ObjectPrinter.For<Child>();
        printer.PrintToString(child).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeArray()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var people = new Person[]
        {
            new() { Name = "Alex", Age = 19, Height = 180, Id = new Guid() },
            new() { Name = "John", Age = 20, Height = 190, Id = new Guid() }
        };
        var printer = ObjectPrinter.For<Person[]>();
        printer.PrintToString(people).Should()
            .Be(expected);
    }

    [Test]
    public void ObjectPrinter_Should_SerializeObject_WithExcludedField()
    {
        var expected = File.ReadAllText($"ExpectedResults/{TestContext.CurrentContext.Test.MethodName}.txt");

        var person = new SkilledPerson
        {
            Name = "Alex", Age = 19, Height = 180, Id = new Guid(), Skill = "C#"
        };
        var printer = ObjectPrinter.For<SkilledPerson>()
            .Exclude(p => p.Skill);
        printer.PrintToString(person).Should().Be(expected);
    }
}