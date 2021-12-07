using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private string result;

        [SetUp]
        public void ClearResult()
        {
            result = "";
        }

        [TearDown]
        public void PrintResult()
        {
            Console.WriteLine(result);
        }
        
        [Test]
        public void TypeExcludingTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Id = new Guid() };
            var printer = ObjectPrinter.For<Person>().Excluding<string>();
            result = printer.PrintToString(person);
            result.Should().NotMatchRegex("Name = Alex");
        }
        
        [Test]
        public void TypeCustomPrintingTest()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(a => a + " years");
            result = printer.PrintToString(person);
            result.Should().MatchRegex("19 years");
        }
        
        [Test]
        public void SetCultureInfoTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture); 
            result = printer.PrintToString(person);
            result.Should().MatchRegex("1\\.85"); 
        }

        [Test]
        public void ExcludePropertyTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);
            result = printer.PrintToString(person);
            result.Should().NotMatchRegex("Id = *");
        }
        
        [Test]
        public void ExcludeFieldTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Employed);
            result = printer.PrintToString(person);
            result.Should().NotMatchRegex("Employed = *");
        }
        
        [Test]
        public void CustomPrintPropertyTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Id)
                .Using(p => "qwerty");
            result = printer.PrintToString(person);
            result.Should().MatchRegex("Id = qwerty");
        }
        
        [Test]
        public void CustomPrintFieldTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Employed).Using(b => b.ToString().ToUpper());
            result = printer.PrintToString(person);
            result.Should().MatchRegex("Employed = FALSE");
        }

        [Test]
        public void StringTrimTest()
        {
            var person = new Person { Name = "12345qwerty", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(5);
            result = printer.PrintToString(person);
            result.Should().MatchRegex("12345").And.NotMatchRegex("qwerty");
        }

        [Test]
        public void CyclicReferenceNotCauseStackOverflowTest()
        {
            var g1 = new GraphObject() { Id = 0 };
            var g2 = new GraphObject() { Id = 2, Child = g1};
            g1.Child = g2;
            Action action = () => result = g1.PrintToString();
            action.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrimitiveArrayPrintingTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.85, Numbers = new []{12, 23, 34}};
            result = person.PrintToString();
        }
        
        [Test]
        public void ReferenceArrayPrintingTest()
        {
            var person1 = new Person { Name = "Alex", Age = 19, Height = 1.85};
            var person2 = new Person { Name = "John", Age = 19, Height = 1.85};
            var person3 = new Person { Name = "Jef", Age = 19, Height = 1.85};
            person1.Friends = new[] { person2, person3 };
            person2.Friends = new[] { person1 };
            person3.Friends = new[] { person1, person2, person3 };
            result = person1.PrintToString();
            result.Should().ContainAll("Alex", "John", "Jef");
        }

        [Test]
        public void DictionaryPrintingTest()
        {
            var dict = new Dictionary<int, string>
            {
                [1] = "24",
                [2] = "qwerty",
                [300] = "hello world",
                [-42] = "Not answer"
            };
            result = dict.PrintToString();
        }

        [Test]
        public void CanPrintStruct()
        {
            var so = new StructObject() { Id = 2, Value = "Hello" };
            result = so.PrintToString();
        }
    }
}