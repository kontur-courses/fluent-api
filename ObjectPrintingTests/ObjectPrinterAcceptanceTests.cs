using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Height = 1.11, Age = 19, Parent = new Person {Name = "Andrew", Age = 90}};
            var persons = new List<Person> {person, person};
            
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing(x => x.Age).Using(x => x > 18 ? "Старый" : "Молодой")
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(2);

            var listPrinter = ObjectPrinter.For<List<Person>>()
                .Excluding<Guid>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru"));

            Console.WriteLine(printer.PrintToString(person));
            Console.WriteLine(person.PrintToString());
            Console.WriteLine(person.PrintToString(s => s.Excluding(p => p.Age)));
            Console.WriteLine(listPrinter.PrintToString(persons));
        }
    }
}