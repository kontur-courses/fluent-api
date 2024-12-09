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
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Settings.UseDirectory("snapshots");
    }
    
    [Test]
    public Task PrintToString_ExcludeType()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };

        var serializedString = PrintingConfig<Person>.Serialize(person, config => config.ExcludeType<int>());

        return Verifier.Verify(serializedString, Settings);
    }
    
    [Test]
    public Task PrintToString_AlternativeTypeSerialization()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };

        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.AddSerializationMethod<DateTime>(_ => "date"));
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_SetCulture()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200.2, Birthday = new DateTime(2000, 10, 10) };

        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.SpecifyTheCulture<double>(new CultureInfo("en-US")));
        
        return Verifier.Verify(serializedString, Settings);
    }
    
    [Test]
    public Task PrintToString_AlternativePropertySerialization()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };

        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.AddSerializationMethod(_ => "NoName", p => p.Name));
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_Trim()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };

        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.Trim(2));
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_ExcludeProperty()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };

        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.ExcludeProperty(p => p.Id));
        
        return Verifier.Verify(serializedString, Settings);
    }
    
    [Test]
    public Task PrintToString_CircularReference()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        person.Father = person;

        var serializedString = PrintingConfig<Person>.Serialize(person);
        
        return Verifier.Verify(serializedString, Settings);
    }

    [Test]
    public Task PrintToString_SerializeArray()
    {
        var persons = new Person[]
        {
            new() { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) },
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
            new() { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) },
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
            { new() { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) }, "first" },
            { new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }, "second" }
        };

        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        return Verifier.Verify(serializedString, Settings);
    }
}