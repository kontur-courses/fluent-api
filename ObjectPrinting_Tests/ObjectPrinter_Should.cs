using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinting_Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 180.2};
        }

        [Test]
        public void ExcludeTypeFromPrinting()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>().PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180,2\r\n");
        }        
        [Test]
        public void ExcludePropertyFromPrinting()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Height).PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n");
        }        
        
        [Test]
        public void CustomizeTypeSerialization()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(s => $"test{s}").PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180,2\r\n\tAge = test19\r\n");
        }       
        
        [Test]
        public void CustomizePropertySerialization()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).Using(s => $"test{s}").PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = testAlex\r\n\tHeight = 180,2\r\n\tAge = 19\r\n");
        }        
        
        [Test]
        public void CustomizeCultureForDoubleType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture).PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180.2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void TrimCustomPropertyToLength()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimToLength(2).PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 180,2\r\n\tAge = 19\r\n");
        }     
        
        [Test]
        public void TrimCustomPropertyToLength_WhenPropertyNameShorter()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimToLength(10).PrintToString(person);
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_Default_WhenCallWithNoArgs()
        {
            var printer = person.PrintToString();
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180,2\r\n\tAge = 19\r\n");
        }        
        
        [Test]
        public void PrintToString_WhenCustomized()
        {
            var printer = person.PrintToString(s => s.Excluding<int>());
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180,2\r\n");
        }

        [Test]
        public void PrintList()
        {
            var list = new List<Person>{person, new Person{Name = "Mad", Age = 25, Height = 155}};
            var printer = list.PrintToString();
            printer.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 180,2\r\n\tAge = 19\r\n" +
                                "Person\r\n\tId = Guid\r\n\tName = Mad\r\n\tHeight = 155\r\n\tAge = 25\r\n");
        }
        
        [Test]
        public void PrintDictionary()
        {
            var list = new Dictionary<int, Person>{{1, person}, {2, new Person{Name = "Mad", Age = 25, Height = 155}}};
            var printer = list.PrintToString();
            printer.Should().Be("KeyValuePair`2\r\n" +
                                "\tKey = 1\r\n" +
                                "\tValue = Person\r\n" +
                                "\t\tId = Guid\r\n" +
                                "\t\tName = Alex\r\n" +
                                "\t\tHeight = 180,2\r\n" +
                                "\t\tAge = 19\r\n" +
                                "KeyValuePair`2\r\n" +
                                "\tKey = 2\r\n" +
                                "\tValue = Person\r\n" +
                                "\t\tId = Guid\r\n" +
                                "\t\tName = Mad\r\n" +
                                "\t\tHeight = 155\r\n" +
                                "\t\tAge = 25\r\n");
        }

        public class A
        {
            public B B { get; set; }
        }

        public class B
        {
            public A A { get; set; }
        }

        [Test]
        public void SetNestingLevel()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            b.A = a;
            var printer = a.PrintToString(s => s.NestingLevel(1));
            printer.Should().Be("A\r\n" +
                                "\tB = B\r\n" +
                                "\t\tA = A\r\n" +
                                "Nesting limit reached");
        }
    }
}