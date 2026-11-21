using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter
                    .For<Person>()
                    .Exclude<Guid>()
                    .Printing<int>()
                    .Using(i => i.ToString("X"))
                    .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                    .Printing(p => p.Age)
                    .Using(age => age + " years")
                    .Printing(p => p.Name)
                    .TrimmedToLength(10)
                    .Exclude(p => p.Height)
                ;
            var expected = string.Join(
                Environment.NewLine,
                "Person",
                "\tName = Alex",
                "\tAge = 19 years");

            string s1 = printer.PrintToString(person);

            s1
                .Should()
                .Be(expected);
        }
    }
}