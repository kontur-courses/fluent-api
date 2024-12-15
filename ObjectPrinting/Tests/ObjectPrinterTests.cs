using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting.Tests;

[TestFixture]
public class PrintingConfigTests
{
    private Person testPerson;

    [SetUp]
    public void SetUp()
    {
        testPerson = new Person
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Name = "John Doe",
            Height = 180.5,
            Age = 30,
            Hobbies = new List<string> { "Reading", "Gaming", "Coding" },
            Scores = new Dictionary<string, int>
            {
                { "Math", 95 },
                { "Physics", 88 }
            }
        };
    }
    
    [Test]
    public void PrintToString_ExcludeProperty_ShouldNotPrintExcludedProperty()
    {
        var result = testPerson.PrintToString(config => 
            config.Excluding(p => p.Age));


        result.Should().Contain("Name = John Doe")
            .And.NotContain("Age = 30");
    }

    [Test]
    public void PrintToString_ExcludeType_ShouldNotPrintPropertiesOfExcludedType()
    {
        var result = testPerson.PrintToString(config => 
            config.Excluding<int>());

        result.Should().Contain("Name = John Doe")
            .And.NotContain("Age = ");
    }

    [Test]
    public void PrintToString_WithCustomTypeSerializer_ShouldUseCustomFormat()
    {
        var result = testPerson.PrintToString(config => 
            config.For<double>()
                .UseSerializer(d => $"{d:F1}m"));
            
        result.Should().Contain("Height = 180,5m");
    }

    [Test]
    public void PrintToString_StringTruncation_ShouldTruncateString()
    {
        var result = testPerson.PrintToString(config => 
            config.For(p => p.Name)
                .SetMaxLength(4));
        
        result.Should().Contain("Name = John");
    }

    [TestCase("de-DE", "180,5")]
    [TestCase("en-US", "180.5")]
    public void PrintToString_WithCulture_ShouldUseCultureFormatting(string cultureName, string expectedHeight)
    {
        var culture = new CultureInfo(cultureName);

        var result = testPerson.PrintToString(config => 
            config.For<double>()
                .SetCulture(culture));
        
        result.Should().Contain($"Height = {expectedHeight}");
    }

    [Test]
    public void PrintToString_WithCollection_ShouldPrintCollectionElements()
    {
        var result = testPerson.PrintToString();

        result.Should().Contain("Reading")
            .And.Contain("Gaming")
            .And.Contain("Coding");
    }

    [Test]
    public void PrintToString_WithCircularReference_ShouldHandleCircularReference()
    {
        var person1 = new Person { Name = "Person1" };
        var person2 = new Person { Name = "Person2" };
        person1.BestFriend = person2;
        person2.BestFriend = person1;

        var result = person1.PrintToString();

        result.Should().Contain("circular reference to Person");
    }

    [Test]
    public void PrintToString_CustomPropertySerializer_ShouldUseCustomFormat()
    {
        var result = testPerson.PrintToString(config => 
            config.For(p => p.Height)
                .UseSerializer(h => $"{h} centimeters"));
            
        result.Should().Contain("Height = 180,5 centimeters");
    }
    [Test]
    public void PrintToString_Dictionary_ShouldPrintKeyValuePairs()
    {
        var result = testPerson.PrintToString();

        result.Should().Contain("Scores = Dictionary {")
            .And.Contain("Math : 95")
            .And.Contain("Physics : 88")
            .And.Contain("}");
    }
    
    [Test]
    public void PrintToString_CustomDictionaryValueSerializer_ShouldUseCustomFormat()
    {
        var result = testPerson.PrintToString(config => 
            config.For(p => p.Scores)
                .UseSerializer(dict => 
                {
                    var scores = dict;
                    return "Dictionary {\n" + string.Join("\n", 
                        scores.Select(kv => $"\t\t{kv.Key} : {kv.Value}%")) + "\n\t}";
                }));

        result.Should().Contain("Math : 95%")
            .And.Contain("Physics : 88%")
            .And.NotContain("Age = 30%");
    }
    [Test]
    public void PrintToString_DictionaryWithComplexKey_ShouldHandleComplexKeys()
    {
        var personWithComplexDictionary = new Person
        {
            Scores = new Dictionary<string, int>
            {
                { "Very Long Subject Name That Should Be Truncated", 100 }
            }
        };

        var result = personWithComplexDictionary.PrintToString(config => 
            config.For(p => p.Scores)
                .SetMaxLength(10));

        result.Should().Contain("Very Long");
    }

    [Test]
    public void PrintToString_NestedDictionary_ShouldHandleNestedStructures()
    {
        var personWithNestedDict = new Person
        {
            Name = "Test Person",
            Scores = new Dictionary<string, int>
            {
                { "Course1", 95 }
            },
            BestFriend = new Person
            {
                Scores = new Dictionary<string, int>
                {
                    { "Course2", 90 }
                }
            }
        };

        var result = personWithNestedDict.PrintToString();

        result.Should().Contain("Course1 : 95")
            .And.Contain("Course2 : 90");
    }
    
    [Test]
    public void PrintToString_DictionaryAndCollectionCombined_ShouldPrintBothCorrectly()
    {
        var result = testPerson.PrintToString();

        result.Should().Contain("Hobbies = Collection [")
            .And.Contain("Scores = Dictionary {")
            .And.Contain("Math : 95")
            .And.Contain("Reading")
            .And.Contain("Gaming");
    }

    [Test]
    public void PrintToString_EmptyDictionary_ShouldHandleEmptyDictionary()
    {
        testPerson.Scores = new Dictionary<string, int>();

        var result = testPerson.PrintToString();

        result.Should().Contain("Scores = Dictionary {")
            .And.Contain("}");
    }

    [Test]
    public void PrintToString_NullDictionary_ShouldHandleNullDictionary()
    {
        testPerson.Scores = null;

        var result = testPerson.PrintToString();

        result.Should().Contain("Scores = null");
    }
    
    [Test]
    public void PrintToString_BasicProperties_ShouldPrintAllPublicProperties()
    {
        var result = testPerson.PrintToString();

        result.Should().Contain("Name = John Doe")
            .And.Contain("Height = 180,5")
            .And.Contain("Age = 30")
            .And.Contain("Id = 12345678-1234-1234-1234-123456789012");
    }
}