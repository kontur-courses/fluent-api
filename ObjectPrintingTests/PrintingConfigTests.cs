using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Test;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrintingComprehensiveTests
{
    private Person firstPerson;
    private Person secondPerson;
    private Family family;

    [SetUp]
    public void Setup()
    {
        firstPerson = new()
        {
            Name = "Ben",
            Surname = "Big",
            Height = 170.1,
            Age = 20,
            BestFriend = new() { Name = "Bob", Surname = "Boby", Height = 40, Age = 80 },
            Friends =
            [
                new() { Name = "Alice", Surname = "Sev", Height = 50, Age = 30 },
                new() { Name = "Max", Surname = "Albor", Height = 10, Age = 9 }
            ],
            BodyParts = { { "Hand", 2 }, { "Foot", 2 }, { "Head", 1 }, { "Tail", 0 } }
        };

        secondPerson = new();
        family = new() { Mom = firstPerson, Dad = secondPerson, Children = [firstPerson, secondPerson] };
    }

    [Test]
    public void PrintToString_ShouldSerializePersonWithAllProperties()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(firstPerson);

        result.Should().Contain("Name = Ben")
              .And.Contain("Surname = Big")
              .And.Contain("Age = 20")
              .And.Contain("Height = 170,1")
              .And.Contain("BestFriend = Person");
    }

    [Test]
    public void PrintToString_ShouldSerializeNestedBestFriend()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(firstPerson);

        result.Should().Contain("BestFriend = Person")
              .And.Contain("Name = Bob")
              .And.Contain("Surname = Boby")
              .And.Contain("Age = 80")
              .And.Contain("Height = 40");
    }

    [Test]
    public void PrintToString_ShouldSerializeFriendsList()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(firstPerson);

        result.Should().Contain("Friends = [")
              .And.Contain("Name = Alice")
              .And.Contain("Name = Max")
              .And.Contain("Surname = Sev")
              .And.Contain("Surname = Albor");
    }

    [Test]
    public void PrintToString_ShouldSerializeBodyPartsDictionary()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(firstPerson);

        result.Should().Contain("BodyParts = [")
              .And.Contain("Hand")
              .And.Contain("Foot")
              .And.Contain("Head")
              .And.Contain("Tail")
              .And.Contain("2")
              .And.Contain("1")
              .And.Contain("0");
    }

    [Test]
    public void ExcludeTypeString_ShouldRemoveNameAndSurname()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .PrintToString(firstPerson);

        result.Should().NotContain("Name =")
              .And.NotContain("Surname =")
              .And.Contain("Age = 20")
              .And.Contain("Height = 170,1");
    }

    [Test]
    public void ExcludePropertyAge_ShouldRemoveOnlyAge()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age)
            .PrintToString(firstPerson);

        result.Should().NotContain("Age =")
              .And.Contain("Name = Ben")
              .And.Contain("Surname = Big")
              .And.Contain("Height = 170,1");
    }

    [Test]
    public void PrintSettingsForInt_ShouldFormatAgeInHex()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<int>()
            .Using(i => i.ToString("X"))
            .PrintToString(firstPerson);

        result.Should().Contain("Age = 14")
              .And.Contain("BodyParts");
    }

    [Test]
    public void PrintPropertySettingsForName_ShouldApplyCustomFormatting()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .Using(name => $"Mr. {name}")
            .PrintToString(firstPerson);

        result.Should().Contain("Name = Mr. Ben")
              .And.Contain("Surname = Big");
    }

    [Test]
    public void TrimmedTo_ShouldShortenName()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .TrimmedTo(2)
            .PrintToString(firstPerson);

        result.Should().Contain("Name = Be")
              .And.Contain("Surname = Big");
    }

    [Test]
    public void UseCultureForDouble_ShouldFormatHeightWithCulture()
    {
        var result = ObjectPrinter.For<Person>()
            .UseCulture<double>(new CultureInfo("en-US"))
            .PrintToString(firstPerson);

        result.Should().Contain("Height = 170.1");
    }

    [Test]
    public void PrintToString_ShouldHandleCircularReferencesInFamily()
    {
        var result = ObjectPrinter.For<Family>().PrintToString(family);

        result.Should().NotContain("StackOverflow")
              .And.NotContain("Infinite loop");
    }

    [Test]
    public void SetMaxNestingLevel_ShouldLimitComplexObjectDepth()
    {
        var result = ObjectPrinter.For<Person>()
            .SetMaxNestingLevel(2)
            .PrintToString(firstPerson);

        result.Should().Contain("max nesting level");
    }

    [Test]
    public void ExcludeBestFriend_ShouldRemoveNestedObject()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.BestFriend)
            .PrintToString(firstPerson);

        result.Should().NotContain("BestFriend = Person")
              .And.Contain("Name = Ben")
              .And.Contain("Friends = [");
    }

    [Test]
    public void ExcludeFriends_ShouldRemoveListButKeepOtherProperties()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Friends)
            .PrintToString(firstPerson);

        result.Should().NotContain("Friends = [")
              .And.Contain("Name = Ben")
              .And.Contain("BestFriend = Person")
              .And.Contain("BodyParts = [");
    }

    [Test]
    public void MultipleExclusions_ShouldWorkTogether()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age)
            .Exclude(p => p.Height)
            .Exclude(p => p.BestFriend)
            .PrintToString(firstPerson);

        result.Should().NotContain("Age =")
              .And.NotContain("Height =")
              .And.NotContain("BestFriend =")
              .And.Contain("Name = Ben")
              .And.Contain("Friends = [")
              .And.Contain("BodyParts = [");
    }

    [Test]
    public void TypeAndPropertyExclusionCombination_ShouldWork()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .Exclude(p => p.Age)
            .PrintToString(firstPerson);

        result.Should().NotContain("Name =")
              .And.NotContain("Surname =")
              .And.NotContain("Age =")
              .And.Contain("Height = 170,1")
              .And.Contain("BestFriend = Person");
    }

    [Test]
    public void PrintSettingsForString_ShouldAffectAllStringProperties()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>()
            .Using(str => str?.ToUpper() ?? "NULL")
            .PrintToString(firstPerson);

        result.Should().Contain("Name = BEN")
              .And.Contain("Surname = BIG")
              .And.Contain("BestFriend = Person")
              .And.Contain("Name = BOB");
    }

    [Test]
    public void ComplexConfiguration_ShouldApplyAllSettings()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .PrintSettings<int>().Using(i => (i * 10).ToString())
            .PrintPropertySettings(p => p.Name).TrimmedTo(1)
            .UseCulture<double>(new CultureInfo("en-US"))
            .Exclude(p => p.Surname)
            .PrintToString(firstPerson);

        result.Should().NotContain("Id")
              .And.NotContain("Surname =")
              .And.Contain("Age = 200")
              .And.Contain("Name = B")
              .And.Contain("Height = 170.1")
              .And.Contain("BestFriend = Person");
    }

    [Test]
    public void PrintToString_ExtensionMethod_ShouldWorkWithFirstPerson()
    {
        var result = firstPerson.PrintToString();

        result.Should().Contain("Name = Ben")
              .And.Contain("Age = 20")
              .And.Contain("BestFriend = Person");
    }

    [Test]
    public void PrintToString_WithConfigExtension_ShouldWork()
    {
        var result = firstPerson.PrintToString(config =>
            config.Exclude(p => p.Age)
                  .PrintPropertySettings(p => p.Name).TrimmedTo(1));

        result.Should().NotContain("Age =")
              .And.Contain("Name = B")
              .And.Contain("Surname = Big");
    }

    [Test]
    public void PrintToString_ShouldHandleEmptySecondPerson()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(secondPerson);

        result.Should().Contain("Person")
              .And.Contain("Name = null")
              .And.Contain("Age = 0")
              .And.Contain("Height = 0")
              .And.Contain("BestFriend = null")
              .And.Contain("Friends = []")
              .And.Contain("BodyParts = []");
    }

    [Test]
    public void PrintToString_ShouldSerializeFamilyStructure()
    {
        var result = ObjectPrinter.For<Family>().PrintToString(family);

        result.Should().Contain("Mom = Person")
              .And.Contain("Dad = Person")
              .And.Contain("Children = [")
              .And.Contain("Name = Ben")
              .And.Contain("Name = null");
    }

    [Test]
    public void PropertySerialization_ShouldOverrideTypeSerializationForSpecificProperty()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>().Using(str => $"TYPE_{str}")
            .PrintPropertySettings(p => p.Name).Using(str => $"PROP_{str}")
            .PrintToString(firstPerson);

        result.Should().Contain("Name = PROP_Ben")
              .And.Contain("Surname = TYPE_Big");
    }

    [Test]
    public void PrintToString_ShouldNotHaveStackOverflowWithComplexStructure()
    {
        var action = () => ObjectPrinter.For<Person>().PrintToString(firstPerson);

        action.Should().NotThrow<StackOverflowException>();
    }

    [Test]
    public void MultipleTypeSerializations_LastOneShouldWin()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>().Using(str => "first")
            .PrintSettings<string>().Using(str => "second")
            .PrintToString(firstPerson);

        result.Should().Contain("Name = second")
              .And.Contain("Surname = second")
              .And.NotContain("first");
    }

    [Test]
    public void MultiplePropertySerializations_LastOneShouldWin()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .Using(name => "first")
            .PrintPropertySettings(p => p.Name)
            .Using(name => "second")
            .PrintToString(firstPerson);

        result.Should().Contain("Name = second")
              .And.NotContain("first");
    }

    [Test]
    public void Exclusion_ShouldHavePriorityOverSerialization()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintSettings<string>().Using(str => "modified")
            .Exclude<string>()
            .PrintToString(firstPerson);

        result.Should().NotContain("Name =")
              .And.NotContain("Surname =")
              .And.NotContain("modified");
    }

    [Test]
    public void PrintToString_ShouldHandleDictionaryValuesCorrectly()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(firstPerson);

        result.Should().Contain("Hand").And.Contain("2")
              .And.Contain("Foot").And.Contain("2")
              .And.Contain("Head").And.Contain("1")
              .And.Contain("Tail").And.Contain("0");
    }

    [Test]
    public void SetMaxNestingLevel_WithValidValue_ShouldNotThrow()
    {
        var action = () => ObjectPrinter.For<Person>()
            .SetMaxNestingLevel(5)
            .PrintToString(firstPerson);

        action.Should().NotThrow();
    }

    [Test]
    public void TrimmedTo_WithValidLength_ShouldNotThrow()
    {
        var action = () => ObjectPrinter.For<Person>()
            .PrintPropertySettings(p => p.Name)
            .TrimmedTo(3)
            .PrintToString(firstPerson);

        action.Should().NotThrow();
    }

    [Test]
    public void PrintToString_ShouldMaintainObjectStructureIntegrity()
    {
        var result = ObjectPrinter.For<Person>().PrintToString(firstPerson);

        result.Should().Contain("Person")
              .And.Contain("BestFriend = Person")
              .And.Contain("Friends = [")
              .And.Contain("BodyParts = [")
              .And.Contain("]");
    }
    
    [Test]
    public void PropertyExclusion_ShouldBeClassSpecific()
    {
        var result = ObjectPrinter.For<Family>()
            .Exclude(f => f.Mom.Name)
            .PrintToString(family);

        var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    
        var momLineIndex = Array.FindIndex(lines, line => line.Trim() == "Mom = Person");
        momLineIndex.Should().BeGreaterThan(-1, "Mom should be present");

        var linesAfterMom = lines.Skip(momLineIndex + 1).Take(10).ToArray();
        var hasMomName = linesAfterMom.Any(line => line.Contains("Name = Ben"));
        hasMomName.Should().BeFalse("Mom.Name should be excluded");
        
        result.Should().Contain("Dad = Person");
    }

    [Test]
    public void PropertySerialization_ShouldBeClassSpecific()
    {
        var result = ObjectPrinter.For<Family>()
            .PrintPropertySettings(f => f.Mom.Name)
            .Using(name => $"MOM_{name}")
            .PrintToString(family);

        result.Should().Contain("MOM_Ben");

        var hasRegularName = result.Contains("Name = null") || result.Contains("Surname = Big");
        hasRegularName.Should().BeTrue("There should be regular non-modified properties");
    }

    [Test]
    public void LargeCollection_ShouldBeLimited()
    {
        var largeList = Enumerable.Range(1, 150).ToList();
        
        var result = ObjectPrinter.For<List<int>>().PrintToString(largeList);

        result.Should().Contain("100")
              .And.Contain("... (showing first 100 items)")
              .And.NotContain("101");
    }

    [Test]
    public void EmptyCollection_ShouldSerializeAsEmpty()
    {
        var emptyList = new List<string>();
        
        var result = ObjectPrinter.For<List<string>>().PrintToString(emptyList);

        result.Should().Be("[]");
    }

    [Test]
    public void CollectionWithFewItems_ShouldShowAll()
    {
        var smallList = new List<int> { 1, 2, 3 };
        
        var result = ObjectPrinter.For<List<int>>().PrintToString(smallList);

        result.Should().Contain("1")
              .And.Contain("2")
              .And.Contain("3")
              .And.NotContain("...");
    }

    [Test]
    public void Dictionary_ShouldBeLimitedWhenLarge()
    {
        var largeDict = new Dictionary<string, int>();
        for (int i = 0; i < 150; i++)
        {
            largeDict[$"key{i}"] = i;
        }
        
        var result = ObjectPrinter.For<Dictionary<string, int>>().PrintToString(largeDict);

        result.Should().Contain("key0")
              .And.Contain("... (showing first 100 items)")
              .And.NotContain("key100");  
    }

    [Test]
    public void NestedCollections_ShouldRespectLimits()
    {
        var nestedList = new List<List<int>>
        {
            Enumerable.Range(1, 50).ToList(),
            Enumerable.Range(51, 60).ToList()
        };
        
        var result = ObjectPrinter.For<List<List<int>>>().PrintToString(nestedList);

        result.Should().Contain("1")
              .And.Contain("50")
              .And.Contain("51")
              .And.Contain("110");
    }
}