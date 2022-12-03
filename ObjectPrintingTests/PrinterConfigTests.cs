using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
public class PrinterConfigTests
{
    [SetUp]
    public void SetUp()
    {
        person = new Person { Name = "Alex", Height = 176.69, Age = 19 };
    }

    private Person person;

    [Test]
    public void PrintAllPublicProperties()
    {
        var printer = ObjectPrinter.For<Person>();
        foreach (var propertyInfo in typeof(Person).GetProperties())
            printer.PrintToString(person).Should().Contain(propertyInfo.Name);
        foreach (var fieldInfo in typeof(Person).GetFields())
            printer.PrintToString(person).Should().Contain(fieldInfo.Name);
    }

    [Test]
    public void Excluding_RemovePropertyOfType()
    {
        var printer = ObjectPrinter.For<Person>().Excluding<int>();
        printer.PrintToString(person).Should().NotContain("Age");
    }

    [Test]
    public void Excluding_RemovePropertyByName()
    {
        var printer = ObjectPrinter.For<Person>().Excluding(x => x.Name);
        printer.PrintToString(person).Should().NotContain("Name");
    }

    [TestCase("en-US")]
    [TestCase("ru-RU")]
    public void UsingCulture(string cultureType)
    {
        var culture = new CultureInfo(cultureType, false);
        var printer = ObjectPrinter.For<Person>().ForType<double>().Use(culture);
        printer.PrintToString(person).Should().Contain(person.Height.ToString(culture));
    }

    [Test]
    public void ApplyForPropertyByName()
    {
        const int length = 3;
        var printer = ObjectPrinter.For<Person>().ForProperty(p => p.Name).TrimToLength(length);
        printer.PrintToString(person).Should()
            .Contain(string.Concat(person.Name.AsSpan(0, length), Environment.NewLine));
    }

    [Test]
    public void ObjectCanPrintItself()
    {
        var printer = ObjectPrinter.For<Person>();
        var s1 = printer.PrintToString(person);
        var s2 = person.PrintToString();
        s2.Should().Be(s1);
    }

    [Test]
    public void PrintToString_NotLooping_OnLoopReferences()
    {
        var loopPerson = new Person();
        loopPerson.Parent1 = loopPerson;
        loopPerson.Parent2 = loopPerson;
        var printer = ObjectPrinter.For<Person>();
        printer.PrintToString(loopPerson);


        Console.WriteLine();

        var p = new Person();
        var p2 = new Person { Parent1 = p };
        p.Parent1 = p2;
        p.Parent2 = p2;
        p2.Parent2 = p2;
        Console.WriteLine(p.PrintToString());
    }

    [Test]
    public void PrintToString_Works_OnListSerialization()
    {
        var list = new List<int>();
        var printer = ObjectPrinter.For<List<int>>();
        printer.PrintToString(list);
    }

    [Test]
    public void PrintingArray()
    {
        var array = new[] { 0, 1, 2, 3, 4, 5 };

        var str = array.PrintToString();
        foreach (var element in array)
            str.Should().Contain(element.ToString());
    }

    [Test]
    public void PrintingList()
    {
        var list = new List<int> { 0, 1, 2, 3, 4, 5 };

        var str = list.PrintToString();
        foreach (var element in list)
            str.Should().Contain(element.ToString());
    }

    [Test]
    public void PrintingDictionary()
    {
        var dictionary = new Dictionary<int, List<string>>
        {
            [0] = new() { "zero", "ноль" },
            [1] = new() { "ont", "один" },
            [2] = new() { "two", "два" },
            [3] = new() { "three", "три" },
            [4] = new() { "four", "четыре" },
            [5] = new() { "five", "пять" }
        };

        var str = dictionary.PrintToString();
        foreach (var pair in dictionary)
        {
            str.Should().Contain($"[{pair.Key}] = ");
            pair.Value.ForEach(value => str.Should().Contain(value));
            foreach (var value in pair.Value)
                str.Should().Contain(value);
        }
    }

    [Test]
    public void PrintNestedTheSameObjects()
    {
        var person1 = new Person();
        var person2 = new Person
        {
            Parent1 = person1,
            Parent2 = person1
        };

        var serializedPerson1 = WithIndention(person1.PrintToString(), 1);
        var serializedPerson2 = person2.PrintToString();
        serializedPerson2.Should()
            .Contain(
                $"Parent1 = {serializedPerson1.Replace(Environment.NewLine, Environment.NewLine + new string('\t', 1))}");
        serializedPerson2.Should()
            .Contain(
                $"Parent2 = {serializedPerson1.Replace(Environment.NewLine, Environment.NewLine + new string('\t', 1))}");
    }

    private static string WithIndention(string str, int level)
    {
        return str.Replace(Environment.NewLine, Environment.NewLine + new string('\t', level));
    }
}