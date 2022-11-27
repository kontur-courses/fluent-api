using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.PrintingConfig;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class ObjectPrinterTests
    {
        [Test]
        public void Should_ExcludeProperty()
        {
            var person = new Person() { Id = new Guid(), Name = "Alex", Age = 17, Height = 1.87 };
            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(p => p.Age);
            
            var result = printer.PrintToString(person);
            
            result.Should().NotContain("Age").And.NotContain("17");
        }
        
        [Test]
        public void Should_ExcludeType()
        {
            var person = new Person() { Id = new Guid(), Name = "Alex", Age = 17, Height = 1.87 };
            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>();
            
            var result = printer.PrintToString(person);
            
            result.Should().NotContain("Age")
                .And.NotContain("17");
        }
        
        [Test]
        public void Should_ExcludeTypeAndProperty()
        {
            var person = new Person() { Id = new Guid(), Name = "Alex", Age = 17, Height = 1.87 };
            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>()
                .ExcludeProperty(p => p.Height);
            
            var result = printer.PrintToString(person);

            result.Should().NotContain("Age")
                .And.NotContain("17")
                .And.NotContain("Height")
                .And.NotContain("1.87");
        }
        
        [Test]
        public void Should_UseProvidedSerializerForProperty()
        {
            var person = new Person() { Id = new Guid(), Name = "Alex", Age = 17, Height = 1.87 };
            var printer = ObjectPrinter.For<Person>()
                .SerializeProperty(p => p.Height)
                    .As(h => (h * 100).ToString(CultureInfo.InvariantCulture));
            
            var result = printer.PrintToString(person);
            
            result.Should().Contain("Height = 187");
        }
        
        [Test]
        public void Should_UseProvidedSerializerForType()
        {
            var person = new Person() { Id = new Guid(), Name = "Alex", Age = 17, Height = 1.87 };
            var printer = ObjectPrinter.For<Person>()
                .SerializeType<Guid>()
                    .As(g => "Globally Unique Identifier");
            
            var result = printer.PrintToString(person);
            
            result.Should().Contain("Id = Globally Unique Identifier");
        }
        
        [Test]
        public void Should_CutStrings()
        {
            var person = new Person() { Id = new Guid(), Name = "Alex", Age = 17, Height = 1.87 };
            var printer = ObjectPrinter.For<Person>()
                .SerializeType<string>()
                    .CutAfter(2);
            
            var result = printer.PrintToString(person);
            
            result.Should().Contain("Name = Al...");
        }
        
        [Test]
        public void Should_SerializeLists()
        {
            var list = new List<string> { "a", "b", "c", "d", "e" };
            var printer = ObjectPrinter.For<List<string>>();
            var result = printer.PrintToString(list);

            var elementsSerialized = new[]
            {
                "[0] = a",
                "[1] = b",
                "[2] = c",
                "[3] = d",
                "[4] = e"
            };
            
            result.Should().ContainAll(elementsSerialized);
        }
        
        [Test]
        public void Should_SerializeDictionaries()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"a", "a"},
                {"b", "b"}, 
                {"c", "c"}, 
                {"d", "d"}, 
                {"e", "e"}
            };
            var printer = ObjectPrinter.For<Dictionary<string, string>>();
            var result = printer.PrintToString(dictionary);

            var elementsSerialized = new[]
            {
                "[a] = a",
                "[b] = b",
                "[c] = c",
                "[d] = d",
                "[e] = e"
            };
            
            result.Should().ContainAll(elementsSerialized);
        }

        [Test]
        public void ShouldNot_BreakOnImmediateCyclicalReferences()
        {
            var person = new Person { Name = "Alex" };
            person.Parent = person;

            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);

            result.Should().Contain("Parent = <Cyclical reference>");
        }
        
        [Test]
        public void ShouldNot_BreakOnNotImmediateCyclicalReferences()
        {
            var person = new Person { Name = "Alex" };
            var parent = new Person { Name = "Bethy" };
            person.Parent = parent;
            parent.Parent = person;

            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);

            result.Should().Contain("Parent = <Cyclical reference>");
        }
    }
}