using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Configs;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    [Test]
    public void Excluding_Guid_OmitsPublicProperty()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var printer = ObjectPrinter.For<Person>();
        var serialisedBeforeExcluding = printer.PrintToString(person);

        var serialisedAfterExcluding = printer
            .Excluding<Guid>()
            .PrintToString(person);

        serialisedBeforeExcluding.Should().Contain("\tId = ");

        serialisedAfterExcluding.Should().NotContain("\tId = ");
        serialisedAfterExcluding.Should().Contain("\tName = Alex");
        serialisedAfterExcluding.Should().Contain("\tAge = 19");
    }

    [Test]
    public void Excluding_Guid_OmitsPublicField()
    {
        var person = new PersonWithField { Name = "Alex", Age = 19, Gender = "Male" };
        var printer = ObjectPrinter.For<PersonWithField>();
        var serialisedBeforeExcluding = printer.PrintToString(person);


        var serialisedAfterExcluding = printer
            .Excluding<Guid>()
            .PrintToString(person);

        serialisedBeforeExcluding.Should().Contain("\tId = ");

        serialisedAfterExcluding.Should().NotContain("\tId = ");
        serialisedAfterExcluding.Should().Contain("\tName = Alex");
        serialisedAfterExcluding.Should().Contain("\tAge = 19");
        serialisedAfterExcluding.Should().Contain("\tGender = Male");
    }

    [Test]
    public void Excluding_Age_OmitsPublicProperty()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var printer = ObjectPrinter.For<Person>();
        var serialisedBeforeExcluding = printer.PrintToString(person);

        var serialisedAfterExcluding = printer
            .Excluding(p => p.Age)
            .PrintToString(person);

        serialisedBeforeExcluding.Should().Contain("\tAge = ");

        serialisedAfterExcluding.Should().NotContain("\tAge = ");
        serialisedAfterExcluding.Should().Contain("\tName = Alex");
    }

    [Test]
    public void Excluding_Gender_OmitsPublicField()
    {
        var person = new PersonWithField { Name = "Alex", Age = 19, Gender = "Male" };
        var printer = ObjectPrinter.For<PersonWithField>();
        var serialisedBeforeExcluding = printer.PrintToString(person);

        var serialisedAfterExcluding = printer
            .Excluding(p => p.Gender)
            .PrintToString(person);

        serialisedBeforeExcluding.Should().Contain("\tGender = ");

        serialisedAfterExcluding.Should().NotContain("\tGender = ");
        serialisedAfterExcluding.Should().Contain("\tName = Alex");
        serialisedAfterExcluding.Should().Contain("\tAge = 19");
    }

    [Test]
    public void Printing_ByType_FormatsPublicFieldsAndPropertiesOfThatType()
    {
        var person = new SampleWithIntMembers();

        var result = ObjectPrinter.For<SampleWithIntMembers>()
            .Printing<int>().Using(value => value.ToString("X"))
            .PrintToString(person);

        var lines = result.Split(Environment.NewLine);

        lines.Should().Contain("\tAge = 13");
        lines.Should().Contain("\tScore = 1A");
        lines.Should().Contain("\tName = Alex");
    }

    [Test]
    public void Printing_BySelector_FormatsOnlySelectedMember()
    {
        var person = new SampleWithIntMembers(); // Age = 19, Score = 26

        var result = ObjectPrinter.For<SampleWithIntMembers>()
            .Printing(p => p.Age).Using(value => value.ToString("X"))
            .PrintToString(person);

        var lines = result.Split(Environment.NewLine);

        lines.Should().Contain("\tAge = 13");
        lines.Should().Contain("\tScore = 26");
    }

    [Test]
    public void Printing_BySelector_FormatsOnlySelectedField()
    {
        var sample = new SampleWithIntMembers(); // Score = 26, Age = 19

        var result = ObjectPrinter.For<SampleWithIntMembers>()
            .Printing(x => x.Score).Using(value => value.ToString("X"))
            .PrintToString(sample);

        var lines = result.Split(Environment.NewLine);

        lines.Should().Contain("\tScore = 1A");
        lines.Should().Contain("\tAge = 19");
    }

    [TestCase(true, "1.5")]
    [TestCase(false, "1,5")]
    public void Printing_ByType_UsesSpecifiedCulture(
        bool useInvariantCulture, string expectedHeight)
    {
        var culture = useInvariantCulture
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo("ru-RU");

        var sample = new SampleWithHeight { Height = 1.5 };

        var result = ObjectPrinter.For<SampleWithHeight>()
            .Printing<double>().Using(culture)
            .PrintToString(sample);

        result.Split(Environment.NewLine)
            .Should().Contain($"\tHeight = {expectedHeight}");
    }

    [TestCase("Alexander", "Ale")]
    [TestCase("Alex", "Ale")]
    [TestCase("Ale", "Ale")]
    [TestCase("Al", "Al")]
    public void TrimmedToLength_TrimsOnlySelectedMember(
        string name, string expectedName)
    {
        var person = new PersonWithField { Name = name, Gender = "Male" };

        var result = ObjectPrinter.For<PersonWithField>()
            .Printing(p => p.Name).TrimmedToLength(3)
            .PrintToString(person);

        var lines = result.Split(Environment.NewLine);

        lines.Should().Contain($"\tName = {expectedName}");
        lines.Should().Contain("\tGender = Male");
    }

    [TestCase(false, TestName = "PrintToString_StopsAtSelfReference")]
    [TestCase(true, TestName = "PrintToString_StopsAtCycleThroughAnotherObject")]
    public void PrintToString_StopsAtCycle(bool useSecondNode)
    {
        var first = new Node { Name = "First" };

        first.Next = useSecondNode
            ? new Node { Name = "Second", Next = first }
            : first;

        var result = ObjectPrinter.For<Node>().PrintToString(first);
        var lines = result.Split(Environment.NewLine);

        lines.Should().Contain("\tName = First");

        if (useSecondNode)
            lines.Should().Contain("\t\tName = Second");

        lines.Should().Contain(
            useSecondNode ? "\t\tNext = <cycle>" : "\tNext = <cycle>");
    }

    [Test]
    public void PrintToString_PrintsSharedReferenceInBothBranches()
    {
        var shared = new Node { Name = "Shared" };
        var pair = new NodePair { Left = shared, Right = shared };

        var result = ObjectPrinter.For<NodePair>().PrintToString(pair);
        var lines = result.Split(Environment.NewLine);

        lines.Should().Contain("\tLeft = Node");
        lines.Should().Contain("\tRight = Node");
        lines.Where(line => line == "\t\tName = Shared").Should().HaveCount(2);
        result.Should().NotContain("<cycle>");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Printing_MemberFormatterOverridesTypeFormatter(
        bool registerMemberFirst)
    {
        var printer = ObjectPrinter.For<Sample>();

        if (registerMemberFirst)
        {
            printer.Printing(x => x.Score).Using(value => $"score:{value}");
            printer.Printing<int>().Using(value => $"int:{value}");
        }
        else
        {
            printer.Printing<int>().Using(value => $"int:{value}");
            printer.Printing(x => x.Score).Using(value => $"score:{value}");
        }

        var lines = printer.PrintToString(new Sample()).Split(Environment.NewLine);

        lines.Should().Contain("\tScore = score:26");
        lines.Should().Contain("\tAge = int:19");
    }

    [Test]
    public void Excluding_MemberOverridesFormatterRegisteredAfterIt()
    {
        var result = ObjectPrinter.For<Sample>()
            .Excluding(x => x.Score)
            .Printing(x => x.Score).Using(value => $"score:{value}")
            .Printing<int>().Using(value => $"int:{value}")
            .PrintToString(new Sample());

        var lines = result.Split(Environment.NewLine);

        lines.Any(line => line.StartsWith("\tScore = ", StringComparison.Ordinal))
            .Should().BeFalse();
        lines.Should().Contain("\tAge = int:19");
    }

    [Test]
    public void Excluding_TypeOverridesFormattersRegisteredAfterIt()
    {
        var result = ObjectPrinter.For<Sample>()
            .Excluding<int>()
            .Printing<int>().Using(value => $"int:{value}")
            .Printing(x => x.Score).Using(value => $"score:{value}")
            .PrintToString(new Sample());

        var lines = result.Split(Environment.NewLine);

        lines.Any(line => line.StartsWith("\tScore = ", StringComparison.Ordinal))
            .Should().BeFalse();
        lines.Any(line => line.StartsWith("\tAge = ", StringComparison.Ordinal))
            .Should().BeFalse();
        lines.Should().Contain("\tName = Alex");
    }

    private sealed class Sample
    {
        public readonly int Score = 26;
        public int Age { get; } = 19;
        public string Name { get; } = "Alex";
    }

    private sealed class Node
    {
        public string Name { get; set; } = "";
        public Node? Next { get; set; }
    }

    private sealed class NodePair
    {
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }

    private sealed class SampleWithHeight
    {
        public double Height { get; set; }
    }

    private sealed class SampleWithIntMembers
    {
        public readonly int Score = 26;
        public int Age { get; } = 19;
        public string Name { get; set; } = "Alex";
    }
}