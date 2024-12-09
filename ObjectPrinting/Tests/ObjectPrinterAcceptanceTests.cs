using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void PrintToString_ExcludeType()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        const string expected = """
                                Person
                                	Id = 00000000-0000-0000-0000-000000000000
                                	Name = Bob
                                	Height = 200
                                	Birthday = 10/10/2000 00:00:00
                                	Father = null
        
                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(person, config => config.ExcludeType<int>());

        serializedString.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_AlternativeTypeSerialization()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        const string expected = """
                                Person
                                	Id = 00000000-0000-0000-0000-000000000000
                                	Name = Bob
                                	Height = 200
                                	Age = 20
                                	Birthday = date
                                	Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.AddSerializationMethod<DateTime>(_ => "date"));
        
        serializedString.Should().Be(expected);
    }

    [Test]
    public void PrintToString_SetCulture()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200.2, Birthday = new DateTime(2000, 10, 10) };
        const string expected = """
                                Person
                                	Id = 00000000-0000-0000-0000-000000000000
                                	Name = Bob
                                	Height = 200.2
                                	Age = 20
                                	Birthday = 10/10/2000 00:00:00
                                	Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.SpecifyTheCulture<double>(new CultureInfo("en-US")));
        
        serializedString.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_AlternativePropertySerialization()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        const string expected = """
                                Person
                                	Id = 00000000-0000-0000-0000-000000000000
                                	Name = NoName
                                	Height = 200
                                	Age = 20
                                	Birthday = 10/10/2000 00:00:00
                                	Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.AddSerializationMethod(_ => "NoName", p => p.Name));
        
        serializedString.Should().Be(expected);
    }

    [Test]
    public void PrintToString_Trim()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        const string expected = """
                                Person
                                	Id = 00000000-0000-0000-0000-000000000000
                                	Name = Bo
                                	Height = 200
                                	Age = 20
                                	Birthday = 10/10/2000 00:00:00
                                	Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.Trim(2));
        
        serializedString.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ExcludeProperty()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        const string expected = """
                                Person
                                	Name = Bob
                                	Height = 200
                                	Age = 20
                                	Birthday = 10/10/2000 00:00:00
                                	Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(
            person, 
            config => config.ExcludeProperty(p => p.Id));
        
        serializedString.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_CircularReference()
    {
        var person = new Person { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) };
        person.Father = person;
        const string expected = """
                                Person
                                	Id = 00000000-0000-0000-0000-000000000000
                                	Name = Bob
                                	Height = 200
                                	Age = 20
                                	Birthday = 10/10/2000 00:00:00
                                	Father = Circular Reference

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(person);
        
        serializedString.Should().Be(expected);
    }

    [Test]
    public void PrintToString_SerializeArray()
    {
        var persons = new Person[]
        {
            new() { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) },
            new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }
        };
        
        const string expected = """
                                Person[]:
                                	Person
                                		Id = 00000000-0000-0000-0000-000000000000
                                		Name = Bob
                                		Height = 200
                                		Age = 20
                                		Birthday = 10/10/2000 00:00:00
                                		Father = null
                                	Person
                                		Id = 00000000-0000-0000-0000-000000000000
                                		Name = Bread
                                		Height = 222
                                		Age = 22
                                		Birthday = 11/11/2001 00:00:00
                                		Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        serializedString.Should().Be(expected);
    }

    [Test]
    public void PrintToString_SerializeList()
    {
        var persons = new List<Person>
        {
            new() { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) },
            new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }
        };
        
        const string expected = """
                                List`1:
                                	Person
                                		Id = 00000000-0000-0000-0000-000000000000
                                		Name = Bob
                                		Height = 200
                                		Age = 20
                                		Birthday = 10/10/2000 00:00:00
                                		Father = null
                                	Person
                                		Id = 00000000-0000-0000-0000-000000000000
                                		Name = Bread
                                		Height = 222
                                		Age = 22
                                		Birthday = 11/11/2001 00:00:00
                                		Father = null

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        serializedString.Should().Be(expected);
    }

    [Test]
    public void PrintToString_SerializeDictionary()
    {
        var persons = new Dictionary<Person, string>
        {
            { new() { Name = "Bob", Age = 20, Height = 200, Birthday = new DateTime(2000, 10, 10) }, "first" },
            { new() { Name = "Bread", Age = 22, Height = 222, Birthday = new DateTime(2001, 11, 11) }, "second" }
        };
        
        const string expected = """
                                Dictionary`2:
                                	KeyValuePair`2
                                		Key = Person
                                			Id = 00000000-0000-0000-0000-000000000000
                                			Name = Bob
                                			Height = 200
                                			Age = 20
                                			Birthday = 10/10/2000 00:00:00
                                			Father = null
                                		Value = first
                                	KeyValuePair`2
                                		Key = Person
                                			Id = 00000000-0000-0000-0000-000000000000
                                			Name = Bread
                                			Height = 222
                                			Age = 22
                                			Birthday = 11/11/2001 00:00:00
                                			Father = null
                                		Value = second

                                """;
        
        var serializedString = PrintingConfig<Person>.Serialize(persons);
        
        serializedString.Should().Be(expected);
    }
}