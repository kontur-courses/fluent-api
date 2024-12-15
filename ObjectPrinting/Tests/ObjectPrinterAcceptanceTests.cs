using System;
using System.Globalization;
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

            var printerTask1 = ObjectPrinter.For<Person>()
                .Exclude<Guid>();

            var printerTask2 = ObjectPrinter.For<Person>()
                .WithSerializer<int>(x => x.ToString());

            var printerTask3 = ObjectPrinter.For<Person>()
                    .ConfigurePropertyConfig<double>()
                    .WithCulture(new CultureInfo("de")).ToParentObjectConfig();

            var printerTask4 = ObjectPrinter.For<Person>()
                    .ConfigurePropertyConfig<string>()
                    .WithSerializer<object>(property => $"Name: {property}");

            var printerTask5 = ObjectPrinter.For<Person>()
                    .ConfigurePropertyConfig<string>().Truncate(10);

            var printerTask6 = ObjectPrinter.For<Person>()
                .Exclude(obj => obj.Age);

            
            string s1 = printerTask1.PrintToString(person);
        }
    }
}