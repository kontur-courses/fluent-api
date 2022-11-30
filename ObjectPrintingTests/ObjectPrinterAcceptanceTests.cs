using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void ObjectPrinter_ExcludingType()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();
            var result = printer.PrintToString(person);
            result.Should().NotContain("Id = Guid");
        }
        
        [Test]
        public void ObjectPrinter_ExcludingMember()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Name);
            var result = printer.PrintToString(person);
            result.Should().NotContain("Name = Alex");
        }

        [Test]
        public void ObjectPrinter_UsingCultureInfo()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 178.5};

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.CurrentCulture);
            var result = printer.PrintToString(person);
            result.Should().NotContain("Height = 178.5").And.Contain("Height = 178,5");
        }
        
        [Test]
        public void ObjectPrinter_UsingPropertySettings()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 178.5};

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.ToUpper());
            var result = printer.PrintToString(person);
            result.Should().Contain("Name = ALEX");
        }
    }
}