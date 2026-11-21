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
                .ExcludeType<Guid>()
                .PrintingType<int>()
                .Using(i => i.ToString("X"))
                .PrintingType<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintingMember(p => p.Age)
                .Using(age => age + " years")
                .PrintingMember(p => p.Name)
                .TrimmedToLength(10)
                .ExcludeMember(p => p.Height);
            var expected = string.Join(
                Environment.NewLine,
                "Person",
                "\tName = Alex",
                "\tAge = 19 years");

            var s1 = printer.PrintToString(person);

            s1
                .Should()
                .Be(expected);
        }
    }
}