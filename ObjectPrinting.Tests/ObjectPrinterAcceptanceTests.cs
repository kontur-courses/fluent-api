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
            var person = PersonFactory.Get();
            var printer = ObjectPrinter
                .For<Person>()
                .Exclude<string>()
                .When<int>()
                    .Use(value => $"~{value}~")
                .When<double>()
                    .Use(new CultureInfo("en-GB"));
            printer.PrintToString(person);
        }
    }
}