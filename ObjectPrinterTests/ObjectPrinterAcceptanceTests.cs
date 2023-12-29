using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinterTests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private readonly string newLine = Environment.NewLine;

   [Test]
    public void ObjectPrinter_OnSubsequentPrints_ShouldNotContainCycle()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>();
        var result = printer.PrintToString(person);
        var result2 = printer.PrintToString(person);
        result2.Should().NotContain("Cycle");
    }
    [Test]
    public void ObjectPrinter_WhenExcludingIntType_ShouldPrintObjectWithoutIntProperties()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>();
        var result = printer.Excluding<double>().PrintToString(person);
        result.Should().NotContain(nameof(person.Height));
    }

    [Test]
    public void ObjectPrinter_WhenExcludingHeight_ShouldPrintObjectWithoutIntProperty()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var printer = ObjectPrinter.For<Person>();
        var result = printer.Excluding<int>().PrintToString(person);
        result.Should().NotContain(nameof(person.Age));
    }

    [Test]
    public void ObjectPrinter_WhenPrintingPropertyUsingSomeConditional_ShouldPrintObjectWithModifiedProperty()
    {
        var person = new Person { Name = "Alex", Age = 15 };
        const string nameProperty = nameof(person.Age);
        var printer = ObjectPrinter.For<Person>();
        var result = printer.Printing(x => x.Age).Using(x => nameProperty).PrintToString(person);
        result.Should().Contain($"{nameProperty} = {nameProperty}");
    }

    [Test]
    public void ObjectPrinter_WhenChangingPropertiesByType_ShouldBeObjectWithModifiedProperty()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 1.2 };

        var printer = ObjectPrinter.For<Person>();
        var result = printer.Printing<double>().Using(x => (x * x).ToString()).PrintToString(person);
        result.Should().Contain($"{nameof(person.Height)} = {person.Height * person.Height}");
    }

    [Test]
    public void PrintToString_WhenPrintAllMembersInObject_ShouldPrintAllPublicMembers()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 1.2 };
        var printer = ObjectPrinter.For<Person>();
        var result = printer.PrintToString(person);
        foreach (var property in person.GetType().GetProperties())
        {
            result.Should().Contain($"{property.Name}");
        }

        foreach (var field in person.GetType().GetFields())
        {
            result.Should().Contain($"{field.Name}");
        }
    }

    [Test]
    public void ObjectPrinter_WhenCultureIsSet_ShouldPrintPropertyWithCulture()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 2.4 };
        var culture = new CultureInfo("en-GB");
        var printer = ObjectPrinter.For<Person>();
        var result = printer.Printing(x => x.Height).Using(culture).PrintToString(person);
        result.Should().Contain($"{nameof(person.Height)} = {person.Height.ToString(culture)}");
    }

    [Test]
    public void ObjectPrinter_WhenTrimmedStringProperties_ShouldPrintObjectWithTrimmedProperties()
    {
        const int maxLen = 1;
        var person = new Person { Name = "Alex", Age = 15, Height = 2.4, Id = Guid.Empty };
        var printer = ObjectPrinter.For<Person>();
        var result = printer.Printing<string>().TrimmedToLength(maxLen).PrintToString(person);
        result.Should().Contain($"{nameof(person.Name)} = {person.Name[..maxLen]}");
    }

    [Test]
    public void ObjectPrinter_WhenTrimmedStringPropertiesButCroppingLengthLess0_ShouldTrowArgumentException()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 2.4, Id = Guid.Empty };
        var printer = ObjectPrinter.For<Person>();
        var action = () => { printer.Printing<string>().TrimmedToLength(-10).PrintToString(person); };
        action.Should().Throw<ArgumentException>("Error: The length of the truncated string cannot be negative");
    }

    [Test]
    public void ObjectPrinter_WhenPropertyRefersItself_ShouldPrintObjectWithCycleProperty()
    {
        var kid = new Kid { Name = "Pasha" };
        var parent = new Kid { Name = "Lev" };
        kid.Parent = parent;
        parent.Parent = kid;


        var printer = ObjectPrinter.For<Kid>();
        var result = printer.PrintToString(kid);
        result.Should().Contain(kid.GetType().Name);
        result.Should().Contain($"\t{nameof(kid.Parent)} = {kid.GetType().Name}");
        result.Should().Contain($"\t\t{nameof(kid.Parent)} = (Cycle)ObjectPrinting.Tests.Kid");
    }

    [Test]
    public void ObjectPrinter_WhenPrintingDictionaryProperty_ShouldPrintObject()
    {
        var dictionary = new Dictionary<int, string>
        {
            { 1, "hello" },
            { 2, "hell" },
            { 3, "hel" },
            { 4, "he" },
            { 5, "h" }
        };
        var collections = new Collections();
        collections.Dictionary = new Dictionary<int, string>(dictionary);
        var printer = ObjectPrinter.For<Collections>();
        var result = printer.PrintToString(collections);
        result.Should().Contain($"{nameof(collections.Dictionary)}");
        foreach (var value in dictionary)
        {
            result.Should().Contain($"{value.Key}{newLine} : {value.Value}{newLine}");
        }
    }

    [Test]
    public void ObjectPrinter_WhenThereIsObjectWithListProperty_ShouldPrintObject()
    {
        var collections = new Collections();
        collections.List = new List<object> { 1, 2, 3 };
        var printer = ObjectPrinter.For<Collections>();
        var result = printer.PrintToString(collections);
        foreach (var value in collections.List)
        {
            result.Should().Contain($"{value}{newLine}");
        }
    }

    [Test]
    public void ObjectPrinter_WhenThereIsArrayGenericObjects_ShouldPrintObject()
    {
        var collections = new Collections();
        collections.Array = new[] { new int[] { 1, 2, 3 } };
        var printer = ObjectPrinter.For<Collections>();
        var result = printer.PrintToString(collections);
        result.Should().Contain($"{nameof(collections.Array)} = {newLine}");
        foreach (var list in collections.Array)
        {
            result.Should().Contain($"\t\t{list.GetType().Name}{newLine}");
            foreach (var value in list) result.Should().Contain($"\t\t\t{value}{newLine}");
        }
    }
    
    [Test]
    public void ObjectPrinter_WhenThereIsEnumerableTypeRefersItself_ShouldPrintObject()
    {
        var collections = new Collections();
        collections.List = new List<object>();
        collections.List.Add(collections.List);
        var printer = ObjectPrinter.For<Collections>();
        var result = printer.PrintToString(collections);
        result.Should().Contain($"{nameof(collections.List)} = {newLine}");
        foreach (var list in collections.List)
        {
            result.Should().Contain($"\t\t\t(Cycle){list.GetType().FullName}");
        }
    }

    [Test]
    public void ObjectPrinter_WhenPrintingSomeClassesInList_ShouldPrintObject()
    {
        var collections = new Collections();
        var child = new Person { Name = "Child" };
        collections.Persons = new List<Person> { new() { Name = "Lev" }, child, child };
        var printer = ObjectPrinter.For<Collections>();
        var result = printer.PrintToString(collections);
        result.Should().Contain($"{nameof(collections.Persons)} = ");
        foreach (var person in collections.Persons)
        {
            result.Should().Contain($"\t\t{person.GetType().Name}");
            result.Should().Contain($"\t\t\t{nameof(person.Name)} = {person.Name}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(person.Age)} = {person.Age}{newLine}");
        }
    }
}