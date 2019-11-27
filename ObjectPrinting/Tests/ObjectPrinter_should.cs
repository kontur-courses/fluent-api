using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinter_should
    {
        private static Person person;
        [SetUp]
        public void SetUp()
        {
            person = new Person();
        }


        [Test]
        public void PrintToString_WhenPersonWithoutArgument()
        {
            var result = person.PrintToString();
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [TestCase("Name", 180, 60)]
        public void PrintToString_WhenPersonWithArgument(string name, int height, int age)
        {
            var guid = new Guid();
            var person = new Person() { Id = guid, Name = name, Height = height, Age = age };
            var result = person.PrintToString();
            result.Should().Be($"Person\r\n	Id = Guid\r\n	Name = {name}\r\n	Height = {height}\r\n	Age = {age}\r\n");
        }

        [Test]
        public void ObjectPrinter_For_WhenExcludingType()
        {
            var result = ObjectPrinter.For<Person>().Excluding<int>().PrintToString(person);
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void PrintToString_WhenExcludingType()
        {
            var result = person.PrintToString(p => p.Excluding<int>());
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_For_WhenExcludingProperty()
        {
            var result = ObjectPrinter.For<Person>().Excluding(p => p.Age).PrintToString(person);
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void PrintToString_WhenExcludingProperty()
        {
            var result = person.PrintToString(ser => ser.Excluding(p => p.Age));
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_For_WhenAlternativePropertySerial()
        {
            var result = ObjectPrinter.For<Person>().AlternativeFor(p => p.Age).Using(a => "{" + a + "}").PrintToString(person);
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = {0}\r\n");
        }

        [Test]
        public void PrintToString_WhenAlternativePropertySerial()
        {
            var result = person.PrintToString(ser => ser.AlternativeFor(p => p.Age).Using(a => "{" + a + "}"));
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = {0}\r\n");
        }

        [Test]
        public void ObjectPrinter_For_WhenAlternativeTypeSerial()
        {
            var result = ObjectPrinter.For<Person>().AlternativeFor<int>().Using(a => "{" + a + "}").PrintToString(person);
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = {0}\r\n");
        }

        [Test]
        public void PrintToString_WhenAlternativeTypeSerial()
        {
            var result = person.PrintToString(ser => ser.AlternativeFor<int>().Using(a => "{" + a + "}"));
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = {0}\r\n");
        }

        [Test]
        public void ObjectPrinter_For_WhenTakeOnlySerial()
        {
            person = new Person(){Name = "Name"};
            var result = ObjectPrinter.For<Person>().AlternativeFor<string>().TakeOnly(1).PrintToString(person);
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = N\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [Test]
        public void PrintToString_WhenTakeOnlySerial()
        {
            person = new Person(){Name = "Name"};
            var result = person.PrintToString(ser => ser.AlternativeFor<string>().TakeOnly(1));
            result.Should().Be("Person\r\n	Id = Guid\r\n	Name = N\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [TestCase("en-GB", 50.5, "50.5")]
        [TestCase("ru-RU", 50.5, "50,5")]
        public void ObjectPrinter_For_WhenAlternativeCultureSerial(string culture, double height, string expectHeight)
        {
            person = new Person { Height = height };
            var result = ObjectPrinter.For<Person>().AlternativeFor<double>().Using(new CultureInfo(culture)).PrintToString(person);
            result.Should().Be($"Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = {expectHeight}\r\n	Age = 0\r\n");
        }

        [TestCase("en-GB",50.5,"50.5")]
        [TestCase("ru-RU", 50.5, "50,5")]
        public void PrintToString_WhenAlternativeCultureSerial(string culture, double height, string expectHeight)
        {
            person = new Person { Height = height };
            var result = person.PrintToString(ser => ser.AlternativeFor<double>().Using(new CultureInfo(culture)));
            result.Should().Be($"Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = {expectHeight}\r\n	Age = 0\r\n");
        }

    }
}