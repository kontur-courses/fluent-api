using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.ObjectPrinter;

namespace ObjectPrinting.ObjectPrinterTests;

[TestFixture]
public class ObjectPrinterStandardSerializationTests
{
    private Person? setUpPerson;

    [OneTimeSetUp]
    public void Setup()
    {
        setUpPerson = new Person
        {
            Id = Guid.Parse("f14dd761-3260-4463-a4ad-6ba14de2026c"),
            Name = "Fai",
            Height = 160.5,
            Age = 20,
            IQ = 120,
            BirthDate = new DateTime(2029, 11, 16, 04, 05, 06, DateTimeKind.Utc),
            Scores = [95, 88, 76],
            Tags = ["brooch", "Nevada"],
            Ratings = new Dictionary<string, int>
            {
                ["quality"] = 5,
                ["speed"] = 4
            },
            Friend = new Person
            {
                Id = Guid.Parse("cd55ace6-ac55-4f85-9434-82672b2fd9ee"),
                Name = "Sigma",
                Height = 180.3,
                Age = 81,
                Scores = [93, 55, 99, 32],
                Tags = ["robots", "moon", "zero"],
                Ratings = new Dictionary<string, int>(),
                Friend = null
            }
        };
    }

    [Test]
    public void PrintToString_ShouldIncludeBasicProperties_WhenPersonIsValid()
    {
        var actual = setUpPerson.PrintToString();
        actual.Should().NotBeNullOrEmpty();
        actual.Should().Contain($"{nameof(setUpPerson.Id)} = {setUpPerson!.Id}");
        actual.Should().Contain($"{nameof(setUpPerson.Name)} = {setUpPerson.Name}");
        actual.Should().Contain($"{nameof(setUpPerson.Height)} = {setUpPerson.Height}");
        actual.Should().Contain($"{nameof(setUpPerson.Age)} = {setUpPerson.Age}");
        actual.Should().Contain($"{nameof(setUpPerson.IQ)} = {setUpPerson.IQ}");
        actual.Should().Contain($"{nameof(setUpPerson.BirthDate)} = {setUpPerson.BirthDate}");
        actual.Should().Contain($"{nameof(setUpPerson.Scores)} = {setUpPerson.Scores.GetType().Name}");
        actual.Should().Contain($"{nameof(setUpPerson.Tags)} = List<String>");
        actual.Should().Contain($"{nameof(setUpPerson.Ratings)} = Dictionary<String, Int32>");
        actual.Should().Contain($"{nameof(setUpPerson.Friend)} = {setUpPerson.Friend!.GetType().Name}");
        actual.Should().Contain($"{nameof(setUpPerson.Friend.Friend)} = {null}");
    }


    [Test]
    public void PrintToString_ShouldApplyMemberExclusion_WhenMemberExcludedInConfig()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Excluding(p => p.Name));

        actual.Should().NotContain($"{nameof(setUpPerson.Name)} = {setUpPerson!.Name}");
        actual.Should().Contain($"{nameof(setUpPerson.Id)} = {setUpPerson!.Id}");
    }

    [Test]
    public void PrintToString_ShouldApplyTypeExclusion_WhenTypeExcludedInConfig()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Excluding<int>());

        actual.Should().NotContain($"{nameof(setUpPerson.Age)} = {setUpPerson!.Age}");
        actual.Should().NotContain($"{nameof(setUpPerson.IQ)} = {setUpPerson!.IQ}");
        actual.Should().Contain($"{nameof(setUpPerson.Id)} = {setUpPerson!.Id}");
    }

    [Test]
    public void PrintToString_ShouldUseCustomFormatter_WhenTypeFormatterIsConfigured()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<int>().Using(n => $"|{n}|"));

        actual.Should().Contain("Age = |20|");
        actual.Should().NotContain("Age = 20");
        actual.Should().Contain("IQ = |120|");
        actual.Should().NotContain("IQ = 120");
    }

    [Test]
    public void PrintToString_ShouldUseCustomFormatter_WhenMemberFormatterIsConfigured()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing(p => p.Age).Using(n => $"Age: {n}"));

        actual.Should().Contain("Age: 20");
        actual.Should().NotContain("Age = 20");
        actual.Should().Contain("IQ = 120");
        actual.Should().NotContain("IQ: 120");
    }

    [Test]
    public void PrintToString_ShouldUseInvariantCulture_WhenDoubleCultureIsSetToInvariant()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<double>().Using(CultureInfo.InvariantCulture));

        actual.Should().Contain($"{nameof(setUpPerson.Height)} = 160.5");
        actual.Should().NotContain($"{nameof(setUpPerson.BirthDate)} = 11/16/2029 04:05:06");
    }

    [Test]
    public void PrintToString_ShouldUseInvariantCulture_WhenDateTimeCultureIsSetToInvariant()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<DateTime>().Using(CultureInfo.InvariantCulture));

        actual.Should().Contain($"{nameof(setUpPerson.BirthDate)} = 11/16/2029 04:05:06");
        actual.Should().NotContain($"{nameof(setUpPerson.Height)} = 160.5");
    }


    [Test]
    public void PrintToString_ShouldApplyMemberSerializer_WhenNameCustomSerializerIsSet()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing(p => p!.Name).Using(p => p.ToUpper()));

        actual.Should().Contain($"{nameof(setUpPerson.Name)} = FAI");
    }

    [Test]
    public void PrintToString_ShouldTrimStringProperty_WhenStringMemberTrimmedToLengthSpecified()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing(p => p.Name).StringTrimmedToLength(2));

        actual.Should().Contain($"{nameof(setUpPerson.Name)} = Fa");
        actual.Should().NotContain($"{nameof(setUpPerson.Name)} = Fai");
    }

    [Test]
    public void PrintToString_ShouldTrimStringProperty_WhenIntMemberTrimmedToLengthSpecified()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing(p => p.IQ).TrimmedToLength(2));

        actual.Should().Contain($"{nameof(setUpPerson.IQ)} = 12");
        actual.Should().NotContain($"{nameof(setUpPerson.IQ)} = 120");
    }

    [Test]
    public void PrintToString_ShouldTrimStringProperty_WhenIntTypeTrimmedToLengthSpecified()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<int>().TrimmedToLength(1));

        actual.Should().Contain($"{nameof(setUpPerson.IQ)} = 1");
        actual.Should().NotContain($"{nameof(setUpPerson.IQ)} = 120");
        actual.Should().Contain($"{nameof(setUpPerson.Age)} = 2");
        actual.Should().NotContain($"{nameof(setUpPerson.Age)} = 20");
    }

    [Test]
    public void PrintToString_ShouldApplyMultipleCustomizations_WhenCombinedExclusionsAndSerializersUsed()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Excluding<Guid>()
            .Excluding(p => p.Tags)
            .Printing<int>().Using(n => $"Number: {n}")
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing(p => p.Name).TrimmedToLength(2)
            .Printing(p => p.Name).Using(p => p.ToUpper()));

        actual.Should().NotContain($"{nameof(setUpPerson.Id)}");
        actual.Should().NotContain($"{nameof(setUpPerson.Tags)}");
        actual.Should().Contain("Number: 20");
        actual.Should().NotContain("Age = 20");
        actual.Should().Contain($"{nameof(setUpPerson.Height)} = 160.5");
        actual.Should().NotContain($"{nameof(setUpPerson.Height)} = 160,5");
        actual.Should().Contain($"{nameof(setUpPerson.Name)} = FA");
    }

    [Test]
    public void PrintToString_ShouldRenderIntArrayWithIndexedItems_WhenPersonHasArray()
    {
        var actual = setUpPerson.PrintToString();

        actual.Should().Contain($"{nameof(setUpPerson.Scores)} = {setUpPerson!.Scores.GetType().Name}");
        actual.Should().Contain($"[0] = {setUpPerson.Scores[0]}");
        actual.Should().Contain($"[1] = {setUpPerson.Scores[1]}");
        actual.Should().Contain($"[2] = {setUpPerson.Scores[2]}");
    }

    [Test]
    public void PrintToString_ShouldRenderGenericListWithIndexedItems_WhenPersonHasList()
    {
        var actual = setUpPerson.PrintToString();

        actual.Should().Contain($"{nameof(setUpPerson.Tags)} = List<String>");
        actual.Should().Contain($"[0] = {setUpPerson.Tags[0]}");
        actual.Should().Contain($"[1] = {setUpPerson.Tags[1]}");
    }

    [Test]
    public void PrintToString_ShouldRenderDictionaryWithKeyValuePairs_WhenPersonHasDictionary()
    {
        var actual = setUpPerson.PrintToString();

        actual.Should().Contain($"{nameof(setUpPerson.Ratings)} = Dictionary<String, Int32>");
        actual.Should().Contain($"Key = {setUpPerson!.Ratings.Keys.First()}");
        actual.Should().Contain($"Value = {setUpPerson.Ratings.Values.First()}");
        actual.Should().Contain($"Key = {setUpPerson.Ratings.Keys.Last()}");
        actual.Should().Contain($"Value = {setUpPerson.Ratings.Values.Last()}");
    }

    [Test]
    public void PrintToString_ShouldRenderMultidimensionalArrayRank_WhenArrayIsTwoDimensional()
    {
        var matrix = new[,]
        {
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        var actual = matrix.PrintToString();

        actual.Should().Contain("Int32[,]");
    }

    [Test]
    public void PrintToString_ShouldRenderJaggedArraySuffixes_WhenArrayIsArrayOfArrays()
    {
        var jagged = new int[][]
        {
            [1, 2, 3],
            [4, 5, 6]
        };

        var actual = jagged.PrintToString();

        actual.Should().Contain("Int32[][]");
    }

    [Test]
    public void PrintToString_ShouldUseCustomSerializer_WhenIntArraySerializerIsConfigured()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<int[]>()
            .Using(arr => $"int[{arr.Length}] {{ {string.Join(", ", arr)} }}"));

        actual.Should().Contain("Scores = int[3] { 95, 88, 76 }");
    }

    [Test]
    public void PrintToString_ShouldUseCustomSerializer_WhenStringListSerializerIsConfigured()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<List<string>>()
            .Using(list => $"List<string> (Count = {list.Count}) [ {string.Join(", ", list)} ]"));

        actual.Should().Contain("Tags = List<string> (Count = 2) [ brooch, Nevada ]");
    }

    [Test]
    public void PrintToString_ShouldUseCustomSerializer_WhenDictionarySerializerIsConfigured()
    {
        var actual = setUpPerson.PrintToString(cfg => cfg
            .Printing<Dictionary<string, int>>()
            .Using(dict =>
            {
                var pairs = dict.Select(kv => $"{kv.Key}: {kv.Value}");
                return $"Dictionary<string,int> (Count = {dict.Count}) {{ {string.Join(", ", pairs)} }}";
            }));

        actual.Should().Contain("Ratings = Dictionary<string,int> (Count = 2) { quality: 5, speed: 4 }");
    }

    [Test]
    public void PrintToString_ShouldEmitCycleMarker_WhenObjectGraphHasCyclicReference()
    {
        var person = new Person();
        person.Friend = person;

        var act = () => person.PrintToString();
        var actual = person.PrintToString();

        act.Should().NotThrow();
        actual.Should().Contain("CyclicRef");
    }

    [Test]
    public void PrintToString_ShouldNotShareVisitedState_WhenDifferentRootsShareSameFriend()
    {
        var sharedFriend = new Person { Name = "Shared", Age = 42 };

        var person1 = new Person { Name = "P1", Friend = sharedFriend };

        var person2 = new Person { Name = "P2", Friend = sharedFriend };

        var actual1 = person1.PrintToString();
        var actual2 = person2.PrintToString();


        actual1.Should().Contain("Friend = Person");
        actual1.Should().NotContain("CyclicRef");

        actual2.Should().Contain("Friend = Person");
        actual2.Should().NotContain("CyclicRef");
    }


    [Test]
    public void PrintToString_ShouldReportNestingLevelExceeded_WhenFriendChainDepthBeyondMax()
    {
        var person = new Person();
        person.Friend = new Person();
        person.Friend.Friend = new Person();

        var printer = new PrintingConfig<Person>
        {
            MaxNestingLevel = 2
        };

        var actual = printer.PrintToString(person);

        actual.Should().Contain("превышен уровень вложенности");
    }
}