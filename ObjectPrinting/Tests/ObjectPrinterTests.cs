using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private Person person = null!;

    [SetUp]
    public void SetUp()
    {
        person = new Person { Name = "Sergey", Surname = "Stakheev", Age = 22, Height = 184 };
    }

    [Test]
    public void PrintToString_PrintsAllPublicPropertiesAndFields()
    {
        var expected = Lines(
            "Person",
            "\tId = 00000000-0000-0000-0000-000000000000",
            "\tName = Sergey",
            "\tHeight = 184",
            "\tAge = 22",
            "\tParent = null",
            "\tFriends = null",
            "\tMarks = null",
            "\tSurname = Stakheev");

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_PrintsNull_ForNullObject()
    {
        var result = ObjectPrinter.For<Person?>().PrintToString(null);

        result.Should().Be("null");
    }

    [Test]
    public void Excluding_Type_RemovesMembersOfThisType()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<string>()
            .PrintToString(person);

        result.Should().NotContain("Name").And.NotContain("Surname").And.Contain("Age = 22");
    }

    [Test]
    public void Excluding_Member_RemovesOnlyThisMember()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(person => person.Name)
            .Excluding(person => person.Surname)
            .PrintToString(person);

        result.Should().NotContain("Name").And.NotContain("Surname").And.Contain("Age = 22");
    }

    [Test]
    public void Excluding_NestedMember_DoesNotAffectSameMemberOfRoot()
    {
        var parent = new Person { Name = "Sergey", Surname = "Stakheev" };
        person.Parent = parent;

        var result = ObjectPrinter.For<Person>()
            .Excluding(person => person.Parent!.Name)
            .PrintToString(person);

        result.Should().Contain("\tName = Sergey").And.NotContain("\t\tName =");
    }

    [Test]
    public void Printing_Type_UsesCustomSerializer()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<int>().Using(number => number.ToString("X"))
            .PrintToString(person);

        result.Should().Contain("Age = 16");
    }

    [Test]
    public void Printing_Type_UsesCulture()
    {
        var culture = new CultureInfo("ru-RU");
        person.Height = 184.5;

        var result = ObjectPrinter.For<Person>()
            .Printing<double>().Using(culture)
            .PrintToString(person);

        result.Should().Contain("Height = 184,5");
    }

    [Test]
    public void Printing_Member_UsesCulture()
    {
        person.Height = 184.5;

        var result = ObjectPrinter.For<Person>()
            .Printing(person => person.Height).Using(CultureInfo.InvariantCulture)
            .PrintToString(person);

        result.Should().Contain("Height = 184.5");
    }

    [Test]
    public void Printing_Member_UsesCustomSerializer()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(person => person.Age).Using(age => $"{age} years")
            .PrintToString(person);

        result.Should().Contain("Age = 22 years");
    }

    [Test]
    public void Printing_Member_HasPriorityOverType()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(person => person.Name).Using(name => name.ToUpper())
            .Printing<string>().Using(_ => "hidden")
            .PrintToString(person);

        result.Should().Contain("Name = SERGEY").And.Contain("Surname = hidden");
    }

    [TestCase(2, "Se")]
    [TestCase(6, "Sergey")]
    [TestCase(10, "Sergey")]
    [TestCase(0, "")]
    public void TrimmedToLength_TrimsOnlySelectedMember(int maxLength, string expectedName)
    {
        var expectedLine = $"\tName = {expectedName}{Environment.NewLine}";

        var result = ObjectPrinter.For<Person>()
            .Printing(person => person.Name).TrimmedToLength(maxLength)
            .PrintToString(person);

        result.Should().Contain(expectedLine).And.Contain("Surname = Stakheev");
    }

    [Test]
    public void TrimmedToLength_ForType_TrimsAllStrings()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<string>().TrimmedToLength(2)
            .PrintToString(person);

        result.Should().Contain("Name = Se").And.Contain("Surname = St");
    }

    [Test]
    public void TrimmedToLength_Throws_WhenLengthIsNegative()
    {
        var act = () => ObjectPrinter.For<Person>().Printing(person => person.Name).TrimmedToLength(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Printing_Throws_WhenSelectorIsNotMember()
    {
        var act = () => ObjectPrinter.For<Person>().Printing(person => person.Age + 1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Excluding_Throws_WhenSelectorReturnsObjectItself()
    {
        var act = () => ObjectPrinter.For<Person>().Excluding(person => person);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void PrintToString_HandlesCyclicReference()
    {
        person.Parent = person;

        var result = person.PrintToString();

        result.Should().Contain("Parent = <cyclic reference>");
    }

    [Test]
    public void PrintToString_HandlesIndirectCyclicReference()
    {
        var parentFriends = new List<Person> { person };
        var parent = new Person { Name = "Sergey", Surname = "Stakheev", Friends = parentFriends };
        person.Parent = parent;

        var result = person.PrintToString();

        result.Should().Contain("<cyclic reference>");
    }

    [Test]
    public void PrintToString_StopsOnMaxNestingLevel_WhenGetterCreatesNewObjects()
    {
        var node = new Node();

        var result = node.PrintToString();

        result.Should().Contain("<max nesting level reached>");
    }

    [Test]
    public void PrintToString_DoesNotTreatRepeatedReferenceAsCycle()
    {
        var friend = new Person { Name = "Sergey", Surname = "Stakheev" };
        person.Friends = new List<Person> { friend, friend };

        var result = person.PrintToString();

        result.Should().NotContain("<cyclic reference>");
    }

    [Test]
    public void PrintToString_PrintsArray()
    {
        var numbers = new[] { 1, 2, 3 };
        var expected = Lines("[", "\t1", "\t2", "\t3", "]");

        var result = numbers.PrintToString();

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_AppliesTypeSerializer_ToCollectionItems()
    {
        var numbers = new[] { 10, 11 };
        var expected = Lines("[", "\tA", "\tB", "]");

        var result = numbers.PrintToString(config => config.Printing<int>().Using(number => number.ToString("X")));

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_PrintsDictionary()
    {
        var marks = new Dictionary<string, int> { ["math"] = 5, ["physics"] = 4 };
        var expected = Lines("{", "\tmath: 5", "\tphysics: 4", "}");

        var result = marks.PrintToString();

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_PrintsNestedCollectionsWithIndentation()
    {
        var friend = new Person { Name = "Sergey", Surname = "Stakheev" };
        person.Friends = new List<Person> { friend };
        person.Marks = new Dictionary<string, int> { ["math"] = 5 };
        var expected = Lines(
            "Person",
            "\tName = Sergey",
            "\tFriends = [",
            "\t\tPerson",
            "\t\t\tName = Sergey",
            "\t\t\tParent = null",
            "\t\t\tFriends = null",
            "\t\t\tMarks = null",
            "\t\t\tSurname = Stakheev",
            "\t]",
            "\tMarks = {",
            "\t\tmath: 5",
            "\t}");

        var result = person.PrintToString(config => config
            .Excluding<Guid>()
            .Excluding<double>()
            .Excluding<int>()
            .Excluding(person => person.Parent)
            .Excluding(person => person.Surname));

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_WithConfig_AppliesConfiguration()
    {
        var result = person.PrintToString(config => config.Excluding(person => person.Age));

        result.Should().NotContain("Age").And.Contain("Name = Sergey");
    }

    private static string Lines(params string[] lines)
    {
        return string.Join(Environment.NewLine, lines);
    }
}