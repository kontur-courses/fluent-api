using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19 };
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ExcludeType()
        {
            var res = printer
                .Exclude<string>()
                .PrintToString(person);

            res.Should().Be("Person\r\n\tId = Guid\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ExcludeName()
        {
            var res = printer
                .Exclude(e => e.Id)
                .PrintToString(person);

            res.Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ForSavesConfiguration()
        {
            var res = printer
                .Exclude(e => e.Id)
                .For<int>()
                .For<Person>()
                .PrintToString(person);

            res.Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n");
        }
    }
}