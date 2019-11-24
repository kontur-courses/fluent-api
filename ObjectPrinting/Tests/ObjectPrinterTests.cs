using System;
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
            var printer = person
                .Serialize()
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
    }
}