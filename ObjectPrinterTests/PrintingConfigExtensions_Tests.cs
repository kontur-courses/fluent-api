using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinterTests
{
    public class PrintingConfigExtensions_Tests
    {
        private Person person;

        [SetUp]
        public void SetUp() => person = new Person { Name = "Alex", Age = 19 , Height = 183};

        [Test]
        public void ExtensionMethod_SerializeByDefault()
        {
            var expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Alex\"\r\n" +
                "Height: 183\r\n" +
                "Age: 19\r\n";
            var stringifyPerson = person.PrintToString();
            stringifyPerson.Should().BeEquivalentTo(expected);
        }
        
        [Test]
        public void ExtensionMethod_SerializeWithConfiguration()
        {
            var expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Alex\"\r\n" +
                "Height: 183\r\n";
            var stringifyPerson = person.PrintToString(s => s.ExcludingProperty(p => p.Age));
            stringifyPerson.Should().BeEquivalentTo(expected);
        }
    }
}