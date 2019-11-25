using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{

    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person = new Person();

        [SetUp]
        public void SetUp()
        {
            person.Id = Guid.NewGuid();
            person.Name = "Gordon";
            person.Height = 178;
            person.Age = 27;
        }
        
        [Test]
        public void ObjectPrinter_Should_BeConfiguredByMethodChaining()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<Guid>()
                .Serialize(p => p.Age)
                .Using(age => age + " old")
                .Serialize(p => p.Name)
                .Using(name => "My name is " + name)
                .Serialize<double>().Using(num => num + " ft");
            var result = printer.PrintToString(person);
            result.Should().Contain("27 old");
            result.Should().Contain("178 ft");
            result.Should().Contain("My name is Gordon");
            result.Should().NotContain(person.Id.ToString());
        }
        
        [Test]
        public void ObjectPrinter_Should_SerializeAnyObjectByDefault()
        {
            var result = person.Serialized();
            result.Should().Contain("Age = 27");
            result.Should().Contain("Height = 178");
            result.Should().Contain("Name = Gordon");
        }
        
        [Test]
        public void ObjectPrinter_Should_SerializeAnyObjectWithConfiguration()
        {
            var result = person
                .Serialize()
                .Excluding<Guid>()
                .Serialize(p => p.Age)
                .Using(age => age + " old")
                .Serialize(p => p.Name)
                .Using(name => "My name is " + name)
                .Serialize<double>()
                .Using(num => num + " ft")
                .PrintToString();
            result.Should().Contain("27 old");
            result.Should().Contain("178 ft");
            result.Should().Contain("My name is Gordon");
            result.Should().NotContain(person.Id.ToString());
        }
        
        [Test]
        public void ObjectPrinter_Should_SerializeLists()
        {
            var list = new List<int> {1, 2, 3, 4};
            var result = list.Serialized();
            result.Should().Contain("1");
            result.Should().Contain("2");
            result.Should().Contain("3");
            result.Should().Contain("4");
        }
        
        [Test]
        public void ObjectPrinter_Should_SerializeArrays()
        {
            var list = new [] {1, 2, 3, 4};
            var result = list.Serialized();
            result.Should().Contain("1");
            result.Should().Contain("2");
            result.Should().Contain("3");
            result.Should().Contain("4");
        }
        
        [Test]
        public void ObjectPrinter_Should_SerializeDictionaries()
        {
            var list = new Dictionary<int, string>() {{1, "a"}, {2, "b"}, {3, "c"}};
            var result = list.Serialized();
            result.Should().Contain("1: a");
            result.Should().Contain("2: b");
            result.Should().Contain("3: c");
        }
        
        [Test]
        public void ObjectPrinter_Should_NotGoDeeperThanNestingLevel()
        {
            var strs1 = new List<List<string>> {new List<string>{"cake"}, new List<string>{"cake"}};
            var strs2 = new List<List<string>> {new List<string>{"cake"}};
            var listsList1 = new List<List<List<string>>> {strs1, strs2};
            var listsList2 = new List<List<List<string>>> {strs1, new List<List<string>>{null}};
            var listsList3 = new List<List<List<string>>> {new List<List<string>>{null}, new List<List<string>>{new List<string>{"cake"}}};
            var list = new Dictionary<int, List<List<List<string>>>>() {{1, listsList1}, {2, listsList2}, {3, listsList3}};
            var result = list.Serialize().NestingLevel(4).PrintToString();
            result.Should().Contain("null");
            result.Should().NotContain("cake");
        }

        [Test]
        public void ObjectPrinter_Should_SerializeCyclicReferencesAccordingToNesting()
        {
            var personsFriend = new Person();
            personsFriend.Age = 22;
            personsFriend.Height = 168;
            personsFriend.Id = Guid.NewGuid();
            personsFriend.Name = "Alyx";
            personsFriend.Friend = person;
            person.Friend = personsFriend;
            var result = person.Serialize().NestingLevel(4).PrintToString();
            result.Should().Contain("Person...");
        }
    }
}