using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using ObjectPrinting.Extensions;
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
        person = new Person
        {
            Name = "Bob", 
            Age = 20, 
            Height = 200, 
            Weight = 100,
            Birthday = new DateTime(2000, 10, 10),
            IsStudent = true
        };
    }
    
    [Test]
    public Task PrintToString_ExcludeType()
    {
        var actual = ObjectPrinter.For<Person>()
            .Excluding<int>()
            .PrintToString(person);

        return Verifier.Verify(actual, Settings);
    }
    
    [Test]
    public Task PrintToString_AlternativeTypeSerialization()
    {
        var actual = ObjectPrinter.For<Person>()
            .Printing<DateTime>()
            .Using(_ => "date")
            .PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_SetCultureForType()
    {
        person.Height = 200.2;
        person.Weight = 100.11;
        var russianCulture = new CultureInfo("ru-RU");

        var actual = ObjectPrinter.For<Person>()
            .Printing<double>()
            .Using(russianCulture)
            .PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_SetCultureForProperty()
    {
        person.Height = 200.2;
        person.Weight = 100.11;
        var russianCulture = new CultureInfo("ru-RU");

        var actual = ObjectPrinter.For<Person>()
            .Printing(p => p.Height)
            .Using(russianCulture)
            .PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }
    
    [Test]
    public Task PrintToString_AlternativePropertySerialization()
    {
        var actual = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .Using(_ => "NoName")
            .PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_Trim()
    {
        var actual = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .TrimmedToLength(2)
            .PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_ExcludeProperty()
    {
        var actual = ObjectPrinter.For<Person>()
            .Excluding(p => p.Id)
            .PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }
    
    [Test]
    public Task PrintToString_CircularReference()
    {
        person.Father = person;

        var actual = ObjectPrinter.For<Person>().PrintToString(person);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_SerializeArray()
    {
        var persons = new[]
        {
            person,
            new()
            {
                Name = "Bread", 
                Age = 22, 
                Height = 222, 
                Weight = 111, 
                Birthday = new DateTime(2001, 11, 11),
                IsStudent = false
            }
        };

        var actual = ObjectPrinter.For<Person[]>().PrintToString(persons);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_SerializeList()
    {
        var persons = new List<Person>
        {
            person,
            new()
            {
                Name = "Bread", 
                Age = 22, 
                Height = 222, 
                Weight = 111, 
                Birthday = new DateTime(2001, 11, 11),
                IsStudent = false
            }
        };

        var actual = ObjectPrinter.For<List<Person>>().PrintToString(persons);
        
        return Verifier.Verify(actual, Settings);
    }

    [Test]
    public Task PrintToString_SerializeDictionary()
    {
        var persons = new Dictionary<Person, string>
        {
            { person, "first" },
            { 
                new()
                {
                    Name = "Bread", 
                    Age = 22, 
                    Height = 222, 
                    Weight = 111, 
                    Birthday = new DateTime(2001, 11, 11),
                    IsStudent = false
                }, 
                "second" 
            }
        };

        var actual = ObjectPrinter.For<Dictionary<Person, string>>().PrintToString(persons);
        
        return Verifier.Verify(actual, Settings);
    }
}