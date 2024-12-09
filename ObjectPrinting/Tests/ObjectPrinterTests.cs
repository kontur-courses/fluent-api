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
    private static readonly VerifySettings Settings = new();
    private Person person;
    
    [SetUp]
    public void Setup()
    {
        Settings.UseDirectory("snapshots");
        person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
    }
    
    [Test]
    public Task PrintToString_ExcludeType()
    {
        var serializedString = PrintingConfig<Person>.Serialize(person, config => config.ExcludeType<int>());

        return Verifier.Verify(serializedString, Settings);
    }
    
    [Test]
    public Task PrintToString_AlternativeTypeSerialization()
    {
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.AddSerializationMethod<DateTime>(_ => "date"));
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_SetCulture()
    {
        person.Height = 200.2;
        
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.SpecifyTheCulture<double>(new CultureInfo("en-US")));
        
        return Verifier.Verify(serializedString, Settings);
    }
    
    [Test]
    public Task PrintToString_AlternativePropertySerialization()
    {
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.AddSerializationMethod(_ => "NoName", p => p.Name));
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_Trim()
    {
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.Trim(2));
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_ExcludeProperty()
    {
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.ExcludeProperty(p => p.Id));
        
        return Verifier.Verify(serializedString, Settings);
    }
    
    [Test]
    public Task PrintToString_CircularReference()
    {
        person.Father = person;

        var serializedString = PrintingConfig<Person>.Serialize(person);
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_SerializeArray()
    {
        var persons = new[]
        {
            person,
            new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }
        };

        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_SerializeList()
    {
        var persons = new List<Person>
        {
            person,
            new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }
        };

        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_SerializeDictionary()
    {
        var persons = new Dictionary<Person, string>
        {
            { person, "first" },
            { new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }, "second" }
        };

        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        return Verifier.Verify(serializedString, Settings);
    }
}