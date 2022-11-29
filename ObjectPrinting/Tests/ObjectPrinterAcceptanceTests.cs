using System;
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
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tHeight = 0", "\tAge = 19", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ExcludeName()
        {
            var res = printer
                .Exclude(e => e.Id)
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                 "Person", "\tName = Alex", "\tHeight = 0", "\tAge = 19", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForType()
        {
            var res = printer
                .Printing<string>()
                .Using(s => "строка с кастомной сериализацией")
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tName = строка с кастомной сериализацией", "\tHeight = 0", "\tAge = 19", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForProperty()
        {
            var res = printer
                .Printing(e => e.Age)
                .Using(num => "Изменил только Age")
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 0", "\tAge = Изменил только Age", "");

            res.Should().Be(expected);
        }
    }
}