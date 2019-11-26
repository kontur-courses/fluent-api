using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;
using ObjectPrinting.Solved.Tests;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinter_Should
    {
        [Test]
        public void ObjectPrinter_ShouldPrint()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>();
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintExcludingByType()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintExcludingByName()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>().Excluding(i => i.Age);
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintWithUsingAndPrinting()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<int>()
                .Using(i => "integer")
                .Printing<double>()
                .Using(i => "double");
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = double	Age = integer");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintDoubleWithCultureInfo()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 7.1};
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<double>()
                .Using(CultureInfo.CurrentCulture);
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 7,1	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintSelectedProperties()
        {
            var person = new Person {Name = "Alexandra", Age = 19, Height = 7.1};
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(a => a.Name)
                .Using(a => "Just Alex");
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Just Alex	Height = 7,1\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintTrimmedProperties()
        {
            var person = new Person {Name = "Alexandra", Age = 19, Height = 7.1};
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(a => a.Name)
                .TrimmedToLength(5);
            printer.PrintToString(person).Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alexa	Height = 7,1\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintComplicatedRequests()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 7.1};
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<int>()
                .Using(a => Convert.ToString(a, 2))
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .Excluding(a => a.Id)
                .Printing(a => a.Name)
                .TrimmedToLength(1);
            printer.PrintToString(person).Should().Be("Person\r\n	Name = A	Height = 7.1	Age = 10011");
        }
    }
}