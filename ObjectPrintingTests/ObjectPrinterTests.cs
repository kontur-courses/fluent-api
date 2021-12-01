using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinterTests
{
    private Person person;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        person = new Person
        {
            Age = 19,
            Birthdate = new DateTime(2001, 12, 20),
            Name = "Sergey",
            Surname = "Tretyakov",
            Father = new Person { Name = "Dad", Height = 178 },
            Height = 193.5,
            Id = new Guid()
        };
    }

    [Test]
    public void Should_SerializeCorrectly()
    {
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should()
            .Contain(person.Name)
            .And.Contain(person.Surname)
            .And.Contain(person.Birthdate.ToString())
            .And.Contain(person.Height.ToString())
            .And.Contain(person.Age.ToString())
            .And.Contain(person.Father.Height.ToString())
            .And.Contain(person.Father.Name);
    }

    [Test]
    public void Should_ThrowOnIncorrectExcludeMemberSelector()
    {
        Assert.Throws<ArgumentException>(
            () =>
            {
                ObjectPrinter.For<Person>()
                   .Excluding(p => "P");
            });
    }

    [Test]
    public void Should_ExcludeType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding<string>();

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.NotContain(person.Surname);
    }

    [Test]
    public void Should_ExcludeProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Name);

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.Contain(person.Surname);
    }

    [Test]
    public void Should_ExcludeField()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Age);

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain($"{nameof(person.Age)} = {person.Age}");
    }

    [Test]
    public void Should_ThrowOnIncorrectPrintingMemberSelector()
    {
        Assert.Throws<ArgumentException>(
            () =>
            {
                ObjectPrinter.For<Person>()
                   .Printing(p => "P")
                   .TrimmedToLength(-3);
            });
    }

    [Test]
    public void Should_UseTypeSerializer()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<string>().Using(value => value.Length.ToString());

        var result = printer.PrintToString(person);

        result.Should()
            .Contain($"{nameof(person.Name)} = {person.Name.Length}")
            .And.Contain($"{nameof(person.Surname)} = {person.Surname.Length}");
    }

    [Test]
    public void Should_UsePropertySerializer()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name).Using(name => name.ToUpper());

        var result = printer.PrintToString(person);

        result.Should()
            .Contain($"{nameof(person.Name)} = {person.Name.ToUpper()}")
            .And.Contain($"{nameof(person.Surname)} = {person.Surname}");
    }

    [Test]
    public void Should_UseFieldSerializer()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Age).Using(name => "18+");

        var result = printer.PrintToString(person);

        result.Should()
            .Contain($"{nameof(person.Age)} = 18+")
            .And.NotContain($"{nameof(person.Age)} = {person.Age}");
    }

    [Test]
    public void Should_UseTypeCulture()
    {
        var culture = CultureInfo.CreateSpecificCulture("fr-FR");
        var printer = ObjectPrinter.For<Person>()
            .Printing<DateTime>().Using(culture);

        var result = printer.PrintToString(person);

        result.Should()
            .Contain(person.Birthdate.ToString(culture));
    }

    [Test]
    public void Should_UsePropertyCulture()
    {
        var culture = CultureInfo.CreateSpecificCulture("en-US");
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Height).Using(culture);

        var result = printer.PrintToString(person);

        result.Should()
            .Contain(person.Height.ToString(culture));
    }

    [Test]
    public void Should_UseFieldCulture()
    {
        var culture = CultureInfo.CreateSpecificCulture("fr-FR");
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Birthdate).Using(culture);

        var result = printer.PrintToString(person);

        result.Should()
            .Contain(person.Birthdate.ToString(culture));
    }

    [Test]
    public void Should_TrimStringType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<string>().TrimmedToLength(3);

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.Contain("Ser")
            .And.NotContain(person.Surname)
            .And.Contain("Tre");
    }

    [Test]
    public void Should_TrimMember()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name).TrimmedToLength(3);

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.Contain("Ser")
            .And.Contain(person.Surname);
    }

    [Test]
    public void Should_ThrowOnNegtiveTrimLength()
    {
        Assert.Throws<ArgumentException>(
            () =>
            {
                ObjectPrinter.For<Person>()
                       .Printing(p => p.Name)
                       .TrimmedToLength(-3);
            });
    }

    [Test]
    public void Should_UseLastTypeSetting()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<string>()
            .Using(name => name.ToLower())
            .Printing<string>()
            .Using(name => name.ToUpper());

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.NotContain(person.Name.ToLower())
            .And.Contain(person.Name.ToUpper());
    }

    [Test]
    public void Should_UseLastMemberSetting()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .Using(name => name.ToLower())
            .Printing(p => p.Name)
            .Using(name => name.ToUpper());

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.NotContain(person.Name.ToLower())
            .And.Contain(person.Name.ToUpper());
    }

    [Test]
    public void Should_UseMemberSerializer()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<string>()
            .Using(str => str.ToUpper())
            .Printing(p => p.Name)
            .Using(name => name.ToLower());

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.Contain(person.Name.ToLower())
            .And.NotContain(person.Name.ToUpper());
    }

    [Test]
    public void Should_PrioritizeMemberSerializer()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .Using(name => name.ToLower())
            .Printing<string>()
            .Using(str => str.ToUpper());

        var result = printer.PrintToString(person);

        result.Should()
            .NotContain(person.Name)
            .And.Contain(person.Name.ToLower())
            .And.NotContain(person.Name.ToUpper());
    }

    [Test]
    public void Should_MarkLoopReference()
    {
        person.Father = person;
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should()
            .Contain("Loop reference");
    }

    [Test]
    public void Should_MarkNestedLoopReference()
    {
        person.Father.Father = person;
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should()
            .Contain("Loop reference");
    }

    [Test]
    public void Should_MarkLoopReference_OnEachLevel()
    {
        person.Father.Father = person;
        person.Mother = person;
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should()
            .Contain($"{nameof(person.Father)} = [Loop reference]")
            .And.Contain($"{nameof(person.Mother)} = [Loop reference]");
    }

    [Test]
    public void Should_ClearCache_AfterPrinting()
    {
        var sibling = new Person { Father = person.Father, Mother = person };
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);
        var siblingResult = printer.PrintToString(sibling);

        siblingResult.Should()
            .NotContain("[Loop reference]");
    }

    [Test]
    public void Should_SerializeAsObject()
    {
        var printer = ObjectPrinter.For<Person>();

        var result = person.PrintToString();
        var printerResult = printer.PrintToString(person);

        printerResult.Should()
            .BeEquivalentTo(result);
    }

    [Test]
    public void Should_SerializeAsObjectWithConfig()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Surname)
            .Printing(p => p.Name).TrimmedToLength(2);

        var result = person.PrintToString(config => config
        .Excluding(p => p.Surname)
        .Printing(p => p.Name).TrimmedToLength(2));
        var printerResult = printer.PrintToString(person);

        printerResult.Should()
            .BeEquivalentTo(result);
    }

    [Test]
    public void Should_SerializeArray()
    {
        var array = new[] { 3, 2, 1 };
        var printer = ObjectPrinter.For<int[]>();

        var result = printer.PrintToString(array);

        result.Should()
            .ContainAll(array.Select(element => element.ToString()));
    }

    [Test]
    public void Should_SerializeList()
    {
        var list = new List<int> { 3, 2, 1 };
        var printer = ObjectPrinter.For<List<int>>();

        var result = printer.PrintToString(list);

        result.Should()
            .ContainAll(list.Select(element => element.ToString()));
    }

    [Test]
    public void Should_SerializeDictionary()
    {
        var dictionary = new Dictionary<int, string> { { 1, "3" }, { 2, "2" }, { 3, "1" } };
        var printer = ObjectPrinter.For<Dictionary<int, string>>();

        var result = printer.PrintToString(dictionary);

        result.Should()
            .ContainAll(dictionary.Keys.Select(key => key.ToString()))
            .And.ContainAll(dictionary.Values.Select(value => value.ToString()));
    }

    [Test]
    public void Should_SerializeEmptyCollection()
    {
        var list = new List<int>();
        var printer = ObjectPrinter.For<List<int>>();

        var result = printer.PrintToString(list);

        result.Should().Contain("Empty");
    }

    [Test]
    public void Should_SerializeNestedCollection()
    {
        var list = new List<int> { 3, 2, 1 };
        person.Grades = list;
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should()
            .ContainAll(list.Select(element => element.ToString()))
            .And.Contain($"{nameof(person.Name)} = {person.Name}")
            .And.Contain($"{nameof(person.Surname)} = {person.Surname}");
    }

    [Test]
    public void Should_UseTypeSerializerForCollectionElements()
    {
        var list = new List<int> { 43, 42, 41 };
        Func<int, string> modifier = element => char.ConvertFromUtf32(element);
        var printer = ObjectPrinter.For<List<int>>()
            .Printing<int>().Using(modifier);

        var result = printer.PrintToString(list);

        result.Should()
            .NotContainAll(list.Select(element => element.ToString()))
            .And.ContainAll(list.Select(modifier));
    }

    [Test]
    public void Should_SerializeInfiniteCollection()
    {
        var collection = InfiniteCollection.Get();

        Assert.DoesNotThrow(
            () =>
            {
                collection.PrintToString();
            });
    }
}