using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPriterShould
    {
        [Test]
        public void AcceptanceTest()
        {
            var person = new Person 
            { 
                Name = "Thomas Anderson", 
                Age = 119, 
                Height = 180.4, 
                Id = Guid.NewGuid() 
            };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(6)
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