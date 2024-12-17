using NUnit.Framework;
using System;
using FluentAssertions;
using System.Globalization;
using ObjectPrinting.Extensions;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private Person person;

    [SetUp]
    public void SetUp()
    {
        person = new Person
        {
            Age = 19, 
            Name = "Alex", 
            Height = 173.65, 
            Id = new Guid(), 
            DateOfBirth = new DateTime(1978, 6, 19)
        };
    }

    [Test]
    public void PrintingConfig_Exclude_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }

    [Test]
    public void PrintingConfig_ExcludeAndAddSerialization_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .Printing<string>().Using(_ => "XXX");

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Name)} = XXX");
    }

    [Test]
    public void PrintingConfig_AddSerializationAndExclude_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<string>().Using(_ => "XXX")
            .Exclude<string>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Name)} = XXX");
    }

    [Test]
    public void PrintingConfig_ExcludeCollection_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<List<Person>>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Friends)}");
    }

    [Test]
    public void PrintingConfig_ExcludeSomeTypes_ShouldExcludeGivenTypes()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .Exclude<int>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
        result.Should().NotContain($"{nameof(person.Age)} = {person.Age}");
    }

    [Test]
    public void PrintingConfig_ExcludeTypeSeveralTimes_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .Exclude<Guid>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }

    [Test]
    public void PrintingConfig_CorrectWorksWithTime()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.DateOfBirth)
            .Using(x => x.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.DateOfBirth)} = 19/06/1978");
    }

    [Test]
    public void PrintingConfig_Exclude_ShouldExcludeGivenProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name);

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
    }

    [Test]
    public void PrintingConfig_PrintingType_ShouldUseCustomMethod()
    {
        const string printingType = "int";

        var printer = ObjectPrinter.For<Person>()
            .Printing<int>()
            .Using(_ => printingType);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Age)} = {printingType}");
    }

    [Test]
    public void PrintingConfig_PrintingUseCulture_ShouldUseGivenCulture()
    {
        var culture = CultureInfo.CreateSpecificCulture("fr-FR");

        var printer = ObjectPrinter.For<Person>()
            .Printing<double>()
            .UseCulture(culture);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Age)} = {person.Age.ToString(culture)}");
    }

    [Test]
    public void PrintingConfig_PrintingProperty_ShouldUseCustomMethod()
    {
        const string printingProperty = "Id";

        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Id)
            .Using(_ => printingProperty);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Id)} = {printingProperty}");
    }

    [Test]
    public void PrintingConfig_PrintingTrim_ShouldReturnTrimmedValue()
    {
        const int trimLength = 4;

        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .TrimmedToLength(trimLength);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Name)} = {person.Name[..trimLength]}");
    }

    [Test]
    public void PrintingConfig_ExcludeField_ShouldExcludeGivenProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age);

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Age)} = {person.Age}");
    }

    [Test]
    public void PrintingConfig_PrintCycledObject_ShouldNotFail()
    {
        person.Friend = person;

        var result = person.PrintToString();

        result.Should().Contain("Maximum serialization depth has been reached");
    }

    [Test]
    public void PrintToString_PrintClassWithList_ShouldSerializeList()
    {
        var friends = new List<SimplePerson> { new(), new() };

        SimplePerson simplePerson = new SimplePerson() { Name = "Alex", Id = new Guid() };

        simplePerson.Friends = friends;

        var result = simplePerson.PrintToString();

        result.Should().Contain("[");
        result.Should().Contain("]");
        for (var i = 0; i < friends.Count; i++)
        {
            var friend = friends[i];

            result.Should().Contain($"\t\t\t{nameof(friend.Id)} = {friend.Id}");
            result.Should().Contain($"\t\t\t{nameof(friend.Name)} = {friend.Name}");
        }
    }

    [Test]
    public void PrintToString_PrintClassWithDictionary_ShouldSerializeDictionary()
    {
        var neighbours = new Dictionary<int, SimplePerson>
            { { 12, new SimplePerson() }, { 19, new SimplePerson() } };

        SimplePerson simplePerson = new SimplePerson() { Name = "Alex", Id = new Guid() };

        simplePerson.Neighbours = neighbours;

        var result = simplePerson.PrintToString();

        foreach (var key in neighbours.Keys)
        {
            var neighbour = neighbours[key];

            result.Should().Contain($"\t\t{{{key}: SimplePerson");
            result.Should().Contain($"\t\t\t{nameof(neighbour.Id)} = {neighbour.Id}");
            result.Should().Contain($"\t\t\t{nameof(neighbour.Name)} = {neighbour.Name}");
        }
    }
}