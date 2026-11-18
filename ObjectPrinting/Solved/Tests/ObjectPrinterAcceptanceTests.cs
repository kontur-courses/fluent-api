using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
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
                .Excluding<System.Guid>()
                .Printing<int>()
                    .Using(i => i.ToString("X"))
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name)
                    .TrimmedToLength(10)
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);
            var s2 = person.PrintToString();
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            s1.Should().Contain("Alex").And.NotContain("Age");
            s2.Should().Contain("Alex");
            s3.Should().NotContain("Age");
        }
    }
}