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
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Id = Guid.NewGuid(), Name = "Gordon", Height = 178, Age = 27};
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
            var level4 = new List<object> {"level4"};
            var level3 = new List<object> {level4, "level3"};
            var level2 = new List<object> {level3, "level2"};
            var level1 = new List<object> {level2, "level1"};
            var result = level1.Serialize().NestingLevel(4).PrintToString();
            result.Should().Contain("level3");
            result.Should().NotContain("level4");
        }

        [Test]
        public void ObjectPrinter_Should_SerializeCyclicReferencesAccordingToNesting()
        {
            var person1 = new Person {Name = "person 1"};
            var person2 = new Person {Name = "person 2"};
            var person3 = new Person {Name = "person 3"};

            person1.Friend = person2;
            person2.Friend = person3;
            person3.Friend = person2;
    
            var result = person1.Serialize().PrintToString();
            result.Should().Contain("Person...");
        }
    }
}