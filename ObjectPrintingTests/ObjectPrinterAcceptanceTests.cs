using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrintingTests.TestsElements;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Serialize<Guid>().Using(x => x.ToString())
                .Serialize<int>().WithCulture(CultureInfo.CurrentCulture)
                .Serialize(p => p.Id).Using(x => x.ToByteArray().ToString())
                .Serialize(p => p.Name).Cut(2)
                .Excluding(p => p.Age);
            
            string s1 = printer.PrintToString(person);

            string s2 = person.PrintToString();

            string s3 = person.PrintToString(c => c
                .Excluding(p => p.Age)
                .Serialize(p => p.Name).Cut(3));
        }
    }
}