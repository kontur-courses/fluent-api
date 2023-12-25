using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Parent = new Person {Name = "Andrew", Age = 90}};

            var printer = ObjectPrinter.For<Person>(1)
                .Excluding<Guid>()
                .Printing(x => x.Age).Using(x => x > 18 ? "Старый" : "Молодой")
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(2);

            string s1 = printer.PrintToString(person);
            
            string s2 = person.PrintToString();

            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}