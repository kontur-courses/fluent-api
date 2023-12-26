using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Acceptance()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.2 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Printing<double>()
                .WithCulture<double>(CultureInfo.CurrentCulture)
                .Using(x => (x * 2).ToString())
                .Printing(person => person.Name)
                .Using(name => $"{name}ing")
                .TrimToLength(2)
                .Excluding(person => person.Age)
                .PrintRecursion(() => "Recursion");
        }
    }
}