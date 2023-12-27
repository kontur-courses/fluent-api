using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void AcceptanceTest()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.2 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Printing<double>()
                .WithCulture<double>(CultureInfo.CurrentCulture)
                .Using(x => (x * 2).ToString())
                .Printing(person => person.Name)
                .Using(name => $"{name}ing")
                .TrimToLength(2)
                .Excluding(person => person.Age)
                .WithCyclicLinkMessage(() => "Recursion")
                .WithCyclicLinkIgnored()
                .WithCyclicLinkException();

            var printedPerson = printer.PrintToString(person);

            printedPerson.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\t" +
                                      "Name = Al\r\n\tHeight = 2,4\r\n");
        }
    }
}