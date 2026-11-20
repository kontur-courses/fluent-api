using FluentAssertions;
using ObjectPrinting.Configuration;
using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrintingTests
{
    [Test]
    public void PrintToString_PublicPropertiesAndFields_PrintsNamesAndValues()
    {
        var person = new Person();

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        result.Should().Contain("Name = Alexander");
        result.Should().Contain("City = Ekaterinburg");
        result.Should().Contain("Age = 19");
        result.Should().Contain("Height = 180");
    }

    [Test]
    public void PrintToString_ExcludeType_OmitsMembersOfThatType()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>().Excluding<int>();

        var result = printer.PrintToString(person);

        result.Should().NotContain("Age =");
        result.Should().NotContain("Height =");
        result.Should().Contain("Name = Alexander");
    }

    [Test]
    public void PrintToString_SetTypeSerializer_FormatsAllMembersOfThatType()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>()
            .Printing<int>().Using(value => $"number:{value}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Age = number:19");
        result.Should().Contain("Height = number:180");
    }

    [Test]
    public void PrintToString_SetInvariantCulture_UsesDecimalPoint()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>()
            .Printing<double>().Using(CultureInfo.InvariantCulture);

        var result = printer.PrintToString(person);

        result.Should().Contain("Weight = 75.5" + Environment.NewLine);
    }

    [Test]
    public void PrintToString_SetStringLength_TrimsAllStrings()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>()
            .Printing<string>().TrimmedToLength(4);

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Alex" + Environment.NewLine);
        result.Should().Contain("City = Ekat" + Environment.NewLine);
    }

    [Test]
    public void PrintToString_ExcludeField_OmitsOnlySelectedField()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>().Excluding(p => p.Height);

        var result = printer.PrintToString(person);

        result.Should().NotContain("Height =");
        result.Should().Contain("Age = 19");
    }

    [Test]
    public void PrintToString_SetPropertySerializer_FormatsOnlySelectedProperty()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Age).Using(value => $"age:{value}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Age = age:19");
        result.Should().Contain("Height = 180");
    }

    [Test]
    public void PrintToString_SetPropertyStringLength_TrimsOnlySelectedProperty()
    {
        var person = new Person();
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name).TrimmedToLength(4);

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Alex" + Environment.NewLine);
        result.Should().Contain("City = Ekaterinburg" + Environment.NewLine);
    }

    [Test]
    public void PrintToString_SelfReference_PrintsCycleMarker()
    {
        var person = new Person();
        person.Friend = person;

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        result.Should().Contain("Name = Alexander");
        result.Should().Contain("Friend = [cycle]");
    }

    [Test]
    public void PrintToString_Array_PrintsElements()
    {
        var numbers = new[] { 10, 20 };

        var result = ObjectPrinter.For<int[]>().PrintToString(numbers);

        result.Should().Contain("[0] = 10" + Environment.NewLine);
        result.Should().Contain("[1] = 20" + Environment.NewLine);
    }

    [Test]
    public void PrintToString_List_PrintsElements()
    {
        var names = new List<string> { "Alex", "Anna" };

        var result = ObjectPrinter.For<List<string>>().PrintToString(names);

        result.Should().Contain("[0] = Alex" + Environment.NewLine);
        result.Should().Contain("[1] = Anna" + Environment.NewLine);
    }

    [Test]
    public void PrintToString_Dictionary_PrintsKeysAndValues()
    {
        var ages = new Dictionary<string, int> { ["Alex"] = 19 };

        var result = ObjectPrinter.For<Dictionary<string, int>>()
            .PrintToString(ages);

        result.Should().Contain("Key = Alex" + Environment.NewLine);
        result.Should().Contain("Value = 19" + Environment.NewLine);
    }

    private class Person
    {
        public string Name { get; set; } = "Alexander";
        public string City = "Ekaterinburg";
        public int Age { get; set; } = 19;
        public int Height = 180;
        public double Weight { get; set; } = 75.5;
        public Person? Friend { get; set; }
    }
}
