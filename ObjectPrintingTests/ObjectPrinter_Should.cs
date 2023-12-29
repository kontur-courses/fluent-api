using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Tests;
using Person = ObjectPrinting.Solved.Tests.Person;

namespace ObjectPrintingTests;

public class ObjectPrinter_Should
{
    [Test]
    public void ExcludeMembersOfType()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();

        printer.PrintToString(actual).Should().NotContain("Id");
    }

    [Test]
    public void SpecificSerializeType()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .WithType<double>().SpecificSerialization(x => $"double{x}double");

        printer.PrintToString(actual).Should().Contain("double180,5double");
    }

    [Test]
    public void ChangeNumberCulture()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .NumberCulture<double>(CultureInfo.CurrentCulture);

        printer.PrintToString(actual).Should().Contain("180,5");
    }

    [Test]
    public void SpecificSerializeMember()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .WithField(p => p.Age).SpecificSerialization(p => $"{p}y.o.");

        printer.PrintToString(actual).Should().Contain("19y.o.");
    }

    [Test]
    public void TrimString()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .TrimString(6);

        printer.PrintToString(actual).Should().HaveLength(6);
    }

    [Test]
    public void ExcludeField()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .WithField(p => p.Age).Exclude();

        printer.PrintToString(actual).Should().NotContain("Age");
    }

    [Test]
    public void MixFilters()
    {
        var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .WithType<double>().SpecificSerialization(x => $"double{x}double")
            .NumberCulture<double>(CultureInfo.CurrentCulture)
            .WithField(p => p.Age).SpecificSerialization(p => $"{p}y.o.")
            .WithField(p => p.Name).Exclude();

        printer.PrintToString(actual).Should().Be(
            $"Person{Environment.NewLine}" +
            $"\tHeight = double180,5double{Environment.NewLine}" +
            $"\tAge = 19y.o.{Environment.NewLine}");
    }

    [Test]
    public void ObjectWithNesting()
    {
        var actual = new Car(1, new Wheel(98, 15.5f));
    
        var printer = ObjectPrinter.For<Car>()
            .WithField(c => c.Wheel.Price).SpecificSerialization(price => $"{price}$");
    
        printer.PrintToString(actual).Should().Contain("15,5$");
    }

    [Test]
    public void CycleRefsCheck()
    {
        var parent = new CycleRef { Id = 0, Child = null };
        var child = new CycleRef { Id = 1, Child = null };
        parent.Child = child;
        child.Child = parent;

        var printer = ObjectPrinter.For<CycleRef>()
            .Exclude<int>();

        printer.PrintToString(parent).Should().Be(
            $"CycleRef{Environment.NewLine}" +
            $"\tChild = CycleRef{Environment.NewLine}" +
            $"\t\tChild = cycled... No more this field{Environment.NewLine}");
    }

    [Test]
    public void ListSerialize()
    {
        var col = new List<int>
        {
            5, 2, 19
        };

        var printer = ObjectPrinter.For<List<int>>();

        printer.PrintToString(col).Should().Be(
            $"List`1<Int32>{Environment.NewLine}" +
            $"\t[0] = 5{Environment.NewLine}" +
            $"\t[1] = 2{Environment.NewLine}" +
            $"\t[2] = 19{Environment.NewLine}");
    }

    [Test]
    public void ArraySerialize()
    {
        var arr = new[] { 5, 2, 19 };

        var printer = ObjectPrinter.For<int[]>();

        printer.PrintToString(arr).Should().Be(
            $"Int32[]{Environment.NewLine}" +
            $"\t[0] = 5{Environment.NewLine}" +
            $"\t[1] = 2{Environment.NewLine}" +
            $"\t[2] = 19{Environment.NewLine}");
    }

    [Test]
    public void DictionarySerialize()
    {
        var dict = new Dictionary<string, int>
        {
            { "a", 1 },
            { "b", 7 },
            { "abc", 2 }
        };

        var printer = ObjectPrinter.For<Dictionary<string, int>>();

        printer.PrintToString(dict).Should().Be(
            $"Dictionary`2<Int32>{Environment.NewLine}" +
            $"\t[0] = a : 1{Environment.NewLine}" +
            $"\t[1] = b : 7{Environment.NewLine}" +
            $"\t[2] = abc : 2{Environment.NewLine}");
    }
}