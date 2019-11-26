using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PropertySerializationConfigTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Id = Guid.NewGuid(), Name = "Gordon", Height = 178, Age = 27};
        }

        [Test]
        public void Using_Should_UseAlternativeSerialization()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Serialize<string>()
                .Using(s => "This is " + s);
            var result = printer.PrintToString(person);

            result.Should().Contain("This is Gordon");
        }
        
        [Test]
        public void Using_Should_UseCultureInfoWithDouble()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Serialize<double>()
                .Using(CultureInfo.CreateSpecificCulture("en-US"));
            person.Height = 202667.4;
            var result = printer.PrintToString(person);

            result.Should().Contain("202667.4");
        }
        
        [Test]
        public void Using_Should_SerializeCertainProperties()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Serialize(p => p.Name)
                .Using(name => "Hello, my name is " + name);
            var result = printer.PrintToString(person);

            result.Should().Contain("Hello, my name is Gordon");
        }
        
        [Test]
        public void Using_Should_SerializeCertainStringPropertiesUsingStringSpecificMethods()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Serialize(p => p.Name)
                .Take(3);
            var result = printer.PrintToString(person);

            result.Should().Contain("Gor");
            result.Should().NotContain("Gordon");
        }
        
        [Test]
        public void Using_Should_ExcludePropertiesProperties()
        {
            person.Bag = new Bag(10);

            var result = person.Serialize().Excluding(p => p.Bag.MaxWeight).PrintToString();
            
            result.Should().NotContain("MaxWeight");
        }
        
        [Test]
        public void Using_Should_SerializePropertiesProperties()
        {
            person.Bag = new Bag(10);

            var result = person
                .Serialize()
                .Serialize(p => p.Bag.MaxWeight)
                .Using(str => str + " kg")
                .PrintToString();
            
            result.Should().Contain("10 kg");
        }
    }

    public class Bag
    {
        public int MaxWeight { get;}

        public Bag(int maxWeight)
        {
            MaxWeight = maxWeight;
        }
    }
}