using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private static readonly VerifySettings DefaultSettings = new();
    private Person person;

    [SetUp]
    public void SetUp()
    {
        DefaultSettings.UseDirectory("Snapshots");
        person = new Person
        {
            Id = new Guid("2a5cacd7-a7a6-4c78-9b7c-c3377846cadd"),
            Age = 10,
            Name = "John Doe",
            Height = 189.52,
            DateOfBirth = new DateTime(1985, 9, 9)
        };
    }

    [Test]
    public Task PrintToString_AfterUsingDefault()
    {
        var serializedStr = person.PrintToString();
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintToString_AfterUsingDefaultWithConfiguration()
    {
        var serializedStr = person.PrintToString( c => c.Excluding(p => p.Age));
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }

    [Test]
    public Task PrintConfig_ShouldExcludeGivenType()
    {
        var config = ObjectPrinter.For<Person>()
            .Excluding<Guid>();
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldExcludeGivenProperty()
    {
        var config = ObjectPrinter.For<Person>()
            .Excluding(p => p.Addresses);
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }

    [Test]
    public Task PrintToString_ShouldDisplayDictionaryCorrectly_AfterAttachingDirectoryToObject()
    {
        person.Addresses = new Dictionary<int, string>
        {
            { 1, "London" },
            { 2, "New York" },
            { 3, "Moscow" }
        };
        
        var serializedStr = person.PrintToString();
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintToString_ShouldDisplayCollectionCorrectly_AfterAttachingCollectionToObject()
    {
        person.Children =
        [
            new Person { Name = "Natasha", Age = 8, DateOfBirth = new DateTime(2002, 9 , 9) },
            new Person { Name = "Pasha", Age = 8, DateOfBirth = new DateTime(2002, 9 , 9) },
        ];
        
        var serializedStr = person.PrintToString();
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }

    [Test]
    public Task PrintConfig_ShouldUseCustomSerializerForGivenType()
    {
        var config = ObjectPrinter.For<Person>()
            .Printing<int>().Using(x => $"{x} :^(");
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldUseCustomSerializerForGivenProperty()
    {
        var config = ObjectPrinter.For<Person>()
            .Printing(p => p.DateOfBirth).Using(x => x.ToString("dd/MM/yyyy"));
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldUseGivenCultureForGivenType()
    {
        var config = ObjectPrinter.For<Person>()
            .Printing<double>().SetCulture(new CultureInfo("en-US"));
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldUseGivenCultureForGivenProperty()
    {
        var config = ObjectPrinter.For<Person>()
            .Printing(p => p.Height).SetCulture(new CultureInfo("en-US"));
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldTrimStringType()
    {
        var config = ObjectPrinter.For<Person>()
            .Printing<string>().TrimmedToLength(1);
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldTrimStringProperty()
    {
        var config = ObjectPrinter.For<Person>()
            .Printing(p => p.Name).TrimmedToLength(2);
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }

    [Test]
    public Task PrintToString_ShouldDetectCircularReferences()
    {
        var father = new Person
        {
            Name = "Pavel Doe", 
            Age = 68, 
            DateOfBirth = new DateTime(1954, 9, 9),
            Father = person
        };
        person.Father = father;
        
        var serializedStr = person.PrintToString();
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }

    [Test]
    public Task PrintToString_ShouldDisplayDirectoryCorrectly()
    {
        var dictionary = new Dictionary<int, Person>
        {
            { 1, person },
            { 2, person }
        };
        
        var serializedStr = dictionary.PrintToString();
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }

    [Test]
    public Task PrintToString_ShouldDisplayCollectionCorrectly()
    {
        var collection = new List<Person>
        {
            person,
            person 
        };
        
        var serializedStr = collection.PrintToString();
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldApplyMultipleConfigurationsCorrectly()
    {
        var config = ObjectPrinter.For<Person>()
            .Excluding<Guid>()
            .Printing<int>().Using(x => $"{x} :^(")
            .Printing<double>().SetCulture(new CultureInfo("en-US"))
            .Printing(p => p.DateOfBirth).Using(x => x.ToString("dd/MM/yyyy"))
            .Printing(p => p.Name).TrimmedToLength(5)
            .Excluding(p => p.Addresses);
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
    [Test]
    public Task PrintConfig_ShouldApplyConfigurationToChildrenObject()
    {
        person.Children =
        [
            new Person { Name = "Natasha", Age = 8, DateOfBirth = new DateTime(2002, 9 , 9) },
            new Person { Name = "Pasha", Age = 8, DateOfBirth = new DateTime(2002, 9 , 9) },
        ];
        var config = ObjectPrinter.For<Person>()
            .Excluding<Guid>();
        
        var serializedStr = config.PrintToString(person);
        
        return Verifier.Verify(serializedStr, DefaultSettings);
    }
    
}