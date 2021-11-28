using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void AcceptanceTest()
        {
            var person = new Person
            {
                Name = "Nebuhadnezzar", Age = 68, Height = 190.8, Id = Guid.NewGuid(),
                Salary = 12000, Weight = 66.7
            };
            var brother = new Person
            {
                Name = "Ashurbanipal",
                Age = 73,
                Height = 194.8,
                Id = Guid.NewGuid(),
                Salary = 12000,
                Weight = 66.7,
                Brother = person
            };
            person.Brother = brother;

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimToLength(10)
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);
            string s2 = person.PrintToString();
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}