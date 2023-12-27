using System.Globalization;
using FluentAssertions;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void ObjectPrinter_PrintObjectWithoutIntProperties_WhenExcludingIntType()
    {
        var person = new Person { Name = "Alex", Age = 19 };
    
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.Excluding(x => x.Height).PrintToString(person);
        s1.Should().NotContain("Height");
    }

    [Test]
    public void ObjectPrinter_PrintObjectWithoutIntProperty_WhenExcludingHeight()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.Excluding<int>().PrintToString(person);
        s1.Should().NotContain("Age");
    }

    [Test] public void ObjectPrinter_PrintObjectWithModifiedProperty_PrintingPropertyUsingSomeConditional()
    {
        var person = new Person { Name = "Alex", Age = 15 };
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.Printing(x => x.Age).Using(x=> x.ToString("X")).PrintToString(person);
        s1.Should().Contain("Age = F");
    }
    [Test]
    public void ObjectPrinter_ChangingPropertiesByType_ObjectWithModifiedProperty()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 1.2};
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.Printing<double>().Using(x=> (x * 2).ToString()).PrintToString(person);
        s1.Should().Contain("Height = 2,4");
    }

    [Test]
    public void ObjectPrinter_PrintPropertyWithCulture_WhenCultureIsSet()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 2.4 };
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.Printing(x => x.Height).Using(new CultureInfo("en-GB")).PrintToString(person);
        s1.Should().Contain("Height = 2.4"); 
    }
    [Test]
    public void ObjectPrinter_PrintObjectWithTrimmedProperties_WhenTrimmedStringProperties()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 2.4, Id = Guid.Empty};
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.Printing<string>().TrimmedToLength(1).PrintToString(person);
        s1.Should().Contain("Name = A\r\n\t");
    }
    [Test]
    public void ObjectPrinter_PrintObjectWithModifiedProperty_WhenTrimmedStringPropertiesButCroppingLengthLess0()
    {
        var person = new Person { Name = "Alex", Age = 15, Height = 2.4, Id = Guid.Empty};
        var printer = ObjectPrinter.For<Person>();
        Action action = () =>
        {
            printer.Printing<string>().TrimmedToLength(-10).PrintToString(person);
        };
        action.Should().Throw<ArgumentException>("Error: The length of the truncated string cannot be negative");
    }
    [Test]
    public void ObjectPrinter_PrintObjectWithCycleProperty_WhenPropertyRefersItself()
    {
        var kid = new Kid { Name = "Pasha"};
        var parent = new Kid{Name = "Lev"};
        kid.Parent = parent;
        parent.Parent = kid;
            
            
            
        var printer = ObjectPrinter.For<Kid>();
        var s1 = printer.PrintToString(kid);
        s1.Should().Contain("Parent = (Cycle) ObjectPrinting.Tests.Kid");
    }
    [Test]
    public void ObjectPrinter_PrintingDictionaryProperty_PrintObject()
    {
        var collections = new Collections();
        collections.Dictionary = new Dictionary<int, string>()
        {
            {1, "hello"},
            {2, "hell"},
            {3, "hel"},
            {4, "he"},
            {5, "h"},
        };
        var printer = ObjectPrinter.For<Collections>();
        const string expected = "Dictionary = \r\n\t\t" +
                                "1\r\n : hello\r\n\t\t" +
                                "2\r\n : hell\r\n\t\t" +
                                "3\r\n : hel\r\n\t\t" +
                                "4\r\n : he\r\n\t\t" +
                                "5\r\n : h\r\n\t";
        var s1 = printer.PrintToString(collections);
        s1.Should().Contain(expected);
    }
    [Test]
    public void ObjectPrinter_WhenThereIsObjectWithListProperty_PrintObject()
    {
        var collections = new Collections();
        collections.List = new List<int>() { 1, 2, 3 };
        var printer = ObjectPrinter.For<Collections>();
        const string expected = "List = \r\n\t\t1\r\n\t\t2\r\n\t\t3\r\n\t";
        var s1 = printer.PrintToString(collections);
        s1.Should().Contain(expected);
    }
    [Test]
    public void ObjectPrinter_WhenThereIsArrayGenericObjects_PrintObject()
    {
        var collections = new Collections();
        collections.List = new List<int>() { 1, 2, 3 };
        collections.Array = new [] { collections.List };
        var printer = ObjectPrinter.For<Collections>();
        const string expected = "Array = " +
                                "\r\n\t\tList`1" +
                                "\r\n\t\t1" +
                                "\r\n\t\t2" +
                                "\r\n\t\t3" +
                                "\r\n\tList = " +
                                "\r\n\t\t1" +
                                "\r\n\t\t2" +
                                "\r\n\t\t" +
                                "3\r\n\t";
        var s1 = printer.PrintToString(collections);
        s1.Should().Contain(expected);
    }
    [Test]
    public void ObjectPrinter_PrintingSomeClassesInList_PrintObject()
    {
        var collections = new Collections();
        collections.List = null;
        collections.Array = null;
        collections.Persons = new List<Person> {new Person(){Name = "Lev"} };
        var printer = ObjectPrinter.For<Collections>();
        const string expected = "Persons = \r\n\t\tPerson";
        var s1 = printer.PrintToString(collections);
        s1.Should().Contain(expected);
    }
}